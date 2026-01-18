#!/usr/bin/env python3
"""
Safety Check Hook for Claude Code
==================================
This hook intercepts Bash tool calls and evaluates them for potential danger
using Claude Haiku. It blocks destructive commands that could cause
unrecoverable damage to the system.

Exit codes:
- 0: Allow the command
- 2: Block the command (stderr is shown to Claude)
- Other: Non-blocking error (command proceeds with warning)
"""

import json
import re
import sys
import os

# =============================================================================
# CONFIGURATION
# =============================================================================

# Model to use for safety evaluation
HAIKU_MODEL = "claude-haiku-4-5-20251001"

# Maximum tokens for Haiku response
MAX_TOKENS = 256

# Commands that should ALWAYS be blocked without API call
# These are catastrophically dangerous with no legitimate use case
IMMEDIATE_BLOCKLIST = [
    # Root filesystem destruction
    r"rm\s+(-[a-zA-Z]*f[a-zA-Z]*\s+)?(-[a-zA-Z]*r[a-zA-Z]*\s+)?/\s*$",
    r"rm\s+(-[a-zA-Z]*r[a-zA-Z]*\s+)?(-[a-zA-Z]*f[a-zA-Z]*\s+)?/\s*$",
    r"rm\s+.*\s+/\*",
    r"rm\s+.*--no-preserve-root",

    # Home directory destruction
    r"rm\s+(-[a-zA-Z]*f[a-zA-Z]*\s+)?(-[a-zA-Z]*r[a-zA-Z]*\s+)?~/?$",
    r"rm\s+(-[a-zA-Z]*r[a-zA-Z]*\s+)?(-[a-zA-Z]*f[a-zA-Z]*\s+)?~/?$",
    r"rm\s+(-[a-zA-Z]*f[a-zA-Z]*\s+)?(-[a-zA-Z]*r[a-zA-Z]*\s+)?\$HOME/?$",
    r"rm\s+(-[a-zA-Z]*r[a-zA-Z]*\s+)?(-[a-zA-Z]*f[a-zA-Z]*\s+)?\$HOME/?$",

    # Current directory destruction (dangerous in project roots)
    r"rm\s+(-[a-zA-Z]*f[a-zA-Z]*\s+)?(-[a-zA-Z]*r[a-zA-Z]*\s+)?\./?$",
    r"rm\s+(-[a-zA-Z]*r[a-zA-Z]*\s+)?(-[a-zA-Z]*f[a-zA-Z]*\s+)?\./?$",

    # Filesystem formatting
    r"mkfs\.\w+",

    # Direct disk writes
    r"dd\s+.*if=.*/dev/(sd[a-z]|nvme|hd[a-z])",
    r"dd\s+.*of=/dev/(sd[a-z]|nvme|hd[a-z])",
    r">\s*/dev/sd[a-z]",
    r">\s*/dev/nvme",
    r">\s*/dev/hd[a-z]",

    # Dangerous permissions on root
    r"chmod\s+(-[a-zA-Z]*R[a-zA-Z]*\s+)?777\s+/\s*$",
    r"chmod\s+777\s+(-[a-zA-Z]*R[a-zA-Z]*\s+)?/\s*$",

    # Fork bomb
    r":\(\)\{\s*:\|:&\s*\};:",
]

# Patterns that trigger Haiku evaluation (risky but context-dependent)
RISKY_PATTERNS = [
    r"\brm\b",           # File removal
    r"\bchmod\b",        # Permission changes
    r"\bchown\b",        # Ownership changes
    r"\bmv\b",           # File moves (can overwrite)
    r"\bdd\b",           # Direct disk operations
    r"\bsudo\b",         # Elevated privileges
    r"\b>\s",            # Output redirection (overwrite)
    r"\b>>\s",           # Output redirection (append)
    r"\btruncate\b",     # File truncation
    r"\bshred\b",        # Secure deletion
    r"\bwipefs\b",       # Filesystem wiping
    r"\bkillall\b",      # Mass process killing
    r"\bpkill\b",        # Pattern-based process killing
    r"\brfkill\b",       # Radio device control
    r"\biptables\b",     # Firewall rules
    r"\bsystemctl\b",    # Service management
]

# =============================================================================
# HELPER FUNCTIONS
# =============================================================================

def log_warning(message: str) -> None:
    """Print a warning to stderr (visible to Claude but non-blocking)."""
    print(f"[safety-check] WARNING: {message}", file=sys.stderr)

def log_error(message: str) -> None:
    """Print an error to stderr (blocks the command with exit 2)."""
    print(f"[safety-check] BLOCKED: {message}", file=sys.stderr)

def is_immediately_blocked(command: str) -> tuple[bool, str]:
    """
    Check if a command matches the immediate blocklist.

    Returns:
        (True, reason) if blocked, (False, "") otherwise
    """
    for pattern in IMMEDIATE_BLOCKLIST:
        if re.search(pattern, command, re.IGNORECASE):
            return True, f"Command matches catastrophic pattern: {pattern}"
    return False, ""

def needs_haiku_evaluation(command: str) -> bool:
    """
    Check if a command contains risky patterns that need AI evaluation.
    """
    for pattern in RISKY_PATTERNS:
        if re.search(pattern, command, re.IGNORECASE):
            return True
    return False

def evaluate_with_haiku(command: str) -> dict:
    """
    Call Claude Haiku to evaluate if a command is dangerous.

    Returns:
        dict with keys: dangerous (bool), reason (str), risk_level (str)
    """
    try:
        import anthropic
    except ImportError:
        log_warning("anthropic package not installed, allowing command")
        return {"dangerous": False, "reason": "anthropic package not installed", "risk_level": "unknown"}

    # Check for API key
    if not os.environ.get("ANTHROPIC_API_KEY"):
        log_warning("ANTHROPIC_API_KEY not set, allowing command")
        return {"dangerous": False, "reason": "API key not set", "risk_level": "unknown"}

    client = anthropic.Anthropic()

    prompt = f"""Evaluate if this bash command could cause unrecoverable damage to a system.

Command: {command}

Consider:
1. Could this delete files/directories in a way that's hard to recover?
2. Could this overwrite important data?
3. Could this modify system settings dangerously?
4. Is the scope too broad (e.g., operating on /* or ~ without careful targeting)?
5. Is this a legitimate development/admin task that happens to use risky tools?

Context: This is being run in a software development environment by Claude Code.
Common safe uses include: removing build artifacts, cleaning node_modules,
moving files within a project, etc.

Respond with ONLY valid JSON (no markdown, no explanation):
{{"dangerous": true/false, "reason": "brief explanation", "risk_level": "low/medium/high/critical"}}

Risk levels:
- low: Normal development operations, minimal risk
- medium: Could cause issues but likely recoverable (e.g., deleting some files)
- high: Could cause significant data loss or system issues
- critical: Could destroy system, wipe drives, or cause unrecoverable damage"""

    try:
        response = client.messages.create(
            model=HAIKU_MODEL,
            max_tokens=MAX_TOKENS,
            messages=[{"role": "user", "content": prompt}]
        )

        # Parse the response
        response_text = response.content[0].text.strip()

        # Try to extract JSON if wrapped in markdown
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            json_lines = [l for l in lines if not l.startswith("```")]
            response_text = "\n".join(json_lines)

        result = json.loads(response_text)

        # Validate expected fields
        if not all(k in result for k in ["dangerous", "reason", "risk_level"]):
            log_warning("Haiku response missing fields, allowing command")
            return {"dangerous": False, "reason": "Invalid response format", "risk_level": "unknown"}

        return result

    except json.JSONDecodeError as e:
        log_warning(f"Failed to parse Haiku response: {e}")
        return {"dangerous": False, "reason": "JSON parse error", "risk_level": "unknown"}
    except anthropic.APIError as e:
        log_warning(f"Anthropic API error: {e}")
        return {"dangerous": False, "reason": "API error", "risk_level": "unknown"}
    except Exception as e:
        log_warning(f"Unexpected error: {e}")
        return {"dangerous": False, "reason": "Unknown error", "risk_level": "unknown"}

# =============================================================================
# MAIN HOOK LOGIC
# =============================================================================

def main():
    """Main entry point for the hook."""

    # Read JSON input from stdin
    try:
        input_data = json.load(sys.stdin)
    except json.JSONDecodeError:
        log_warning("Failed to parse input JSON, allowing command")
        sys.exit(0)

    # Only process Bash tool calls
    tool_name = input_data.get("tool_name", "")
    if tool_name != "Bash":
        sys.exit(0)

    # Extract the command
    tool_input = input_data.get("tool_input", {})
    command = tool_input.get("command", "")

    if not command:
        sys.exit(0)

    # Step 1: Check immediate blocklist (no API call needed)
    blocked, reason = is_immediately_blocked(command)
    if blocked:
        log_error(f"{reason}\n\nCommand: {command}\n\nThis command is ALWAYS blocked due to catastrophic potential.")
        sys.exit(2)

    # Step 2: Check if command needs Haiku evaluation
    if not needs_haiku_evaluation(command):
        # Command doesn't match any risky patterns, allow it
        sys.exit(0)

    # Step 3: Evaluate with Haiku
    result = evaluate_with_haiku(command)

    if result["dangerous"]:
        risk_level = result["risk_level"]
        reason = result["reason"]

        if risk_level in ["high", "critical"]:
            log_error(
                f"Command blocked due to {risk_level.upper()} risk.\n\n"
                f"Reason: {reason}\n\n"
                f"Command: {command}\n\n"
                f"If this is intentional, the user must run this command manually."
            )
            sys.exit(2)
        elif risk_level == "medium":
            log_error(
                f"Command blocked due to MEDIUM risk - requires user confirmation.\n\n"
                f"Reason: {reason}\n\n"
                f"Command: {command}\n\n"
                f"Ask the user to confirm they want to run this command."
            )
            sys.exit(2)

    # Command is safe or low risk - allow it
    sys.exit(0)

if __name__ == "__main__":
    main()
