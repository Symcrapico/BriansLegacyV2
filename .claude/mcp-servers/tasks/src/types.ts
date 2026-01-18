// ============================================================================
// types.ts
// Purpose: TypeScript type definitions for task management MCP server
// Dependencies: None
// ============================================================================

// Status emoji constants
export const STATUS = {
  BACKLOG: '⬜',
  READY: '🟦',
  IN_PROGRESS: '🟨',
  BLOCKED: '🟥',
  DONE: '✅',
} as const;

export type StatusType = (typeof STATUS)[keyof typeof STATUS];

export const STATUS_FROM_EMOJI: Record<string, keyof typeof STATUS> = {
  '⬜': 'BACKLOG',
  '🟦': 'READY',
  '🟨': 'IN_PROGRESS',
  '🟥': 'BLOCKED',
  '✅': 'DONE',
};

// Core data structures
export interface Task {
  id: string; // e.g., "TASK-001-A-3"
  description: string;
  status: StatusType;
  estimate?: string; // e.g., "30m"
  started?: string; // ISO timestamp
  completed?: string; // ISO timestamp
  commit?: string; // Git commit hash
  blockedReason?: string;
  featureId: string; // Parent feature
  epicId: string; // Grandparent epic
}

export interface Feature {
  id: string; // e.g., "FEAT-001-A"
  title: string;
  epicId: string;
  tasks: Task[];
  completedCount: number;
  totalCount: number;
}

export interface Epic {
  id: string; // e.g., "EPIC-001"
  title: string;
  priority: string; // e.g., "P1"
  features: Feature[];
  completedCount: number;
  totalCount: number;
}

export interface CompletedTask {
  id: string;
  description: string;
  completed: string; // ISO timestamp or formatted date
  commit?: string;
}

export interface Backlog {
  updated: string; // ISO timestamp
  active: string | null; // Active task ID
  epics: Epic[];
  completed: CompletedTask[];
}

// Tool response types
export interface StatusResponse {
  active: Task | null;
  blocked: Task[];
  next: Task[];
  progress: {
    epic: string;
    epicTitle: string;
    epicProgress: string;
    feature: string;
    featureTitle: string;
    featureProgress: string;
  } | null;
}

// MCP Tool result type
export interface ToolResult {
  content: Array<{ type: 'text'; text: string }>;
}

// Helper to create tool result
export function textResult(text: string): ToolResult {
  return {
    content: [{ type: 'text', text }],
  };
}

export function jsonResult(data: unknown): ToolResult {
  return {
    content: [{ type: 'text', text: JSON.stringify(data, null, 2) }],
  };
}

export function errorResult(message: string): ToolResult {
  return {
    content: [{ type: 'text', text: `Error: ${message}` }],
  };
}
