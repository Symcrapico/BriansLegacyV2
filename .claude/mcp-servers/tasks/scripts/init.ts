// ============================================================================
// init.ts
// Purpose: Guided initialization script for task management system
// Dependencies: fs, path, readline
// ============================================================================

import * as fs from 'fs';
import * as path from 'path';
import * as readline from 'readline';

const BACKLOG_TEMPLATE = `# Backlog

> Updated: {{TIMESTAMP}}
> Active: None

---

## {{EPIC_ID}}: {{EPIC_TITLE}}
**Priority:** P1 | **Progress:** 0/0

### {{FEATURE_ID}}: {{FEATURE_TITLE}} (0/0)

| ID | Task | Status | Est |
|----|------|--------|-----|

---

## Completed

*No completed tasks yet.*
`;

async function prompt(rl: readline.Interface, question: string): Promise<string> {
  return new Promise((resolve) => {
    rl.question(question, (answer) => {
      resolve(answer.trim());
    });
  });
}

async function confirm(rl: readline.Interface, question: string): Promise<boolean> {
  const answer = await prompt(rl, question + ' (y/N): ');
  return answer.toLowerCase() === 'y' || answer.toLowerCase() === 'yes';
}

async function main() {
  const cwd = process.cwd();
  const backlogPath = path.join(cwd, 'BACKLOG.md');
  const tasksDir = path.join(cwd, 'tasks');

  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
  });

  console.log('');
  console.log('┌─────────────────────────────────────────────────────────────┐');
  console.log('│ 📋 Task Management System - Guided Setup                    │');
  console.log('└─────────────────────────────────────────────────────────────┘');
  console.log('');
  console.log('This will create BACKLOG.md and set up the task management system.');
  console.log('');

  // Check if BACKLOG.md already exists
  if (fs.existsSync(backlogPath)) {
    const overwrite = await confirm(rl, 'BACKLOG.md already exists. Overwrite?');
    if (!overwrite) {
      console.log('Aborted. Existing BACKLOG.md preserved.');
      rl.close();
      return;
    }
  }

  // Get epic info
  console.log('');
  console.log('Let\'s set up your first epic (a major feature or initiative):');
  const epicTitle = await prompt(rl, '  Epic title (e.g., "Email Template Migration"): ');
  if (!epicTitle) {
    console.log('Epic title is required. Aborted.');
    rl.close();
    return;
  }

  // Get feature info
  console.log('');
  console.log('Now let\'s add a feature under this epic:');
  const featureTitle = await prompt(rl, '  Feature title (e.g., "RequestEmailService Templates"): ');
  if (!featureTitle) {
    console.log('Feature title is required. Aborted.');
    rl.close();
    return;
  }

  // Create BACKLOG.md
  const now = new Date().toISOString();
  const content = BACKLOG_TEMPLATE
    .replace('{{TIMESTAMP}}', now)
    .replace('{{EPIC_ID}}', 'EPIC-001')
    .replace('{{EPIC_TITLE}}', epicTitle)
    .replace('{{FEATURE_ID}}', 'FEAT-001-A')
    .replace('{{FEATURE_TITLE}}', featureTitle);

  fs.writeFileSync(backlogPath, content);
  console.log('');
  console.log('✅ Created BACKLOG.md');

  // Create tasks directory
  if (!fs.existsSync(tasksDir)) {
    fs.mkdirSync(tasksDir, { recursive: true });
    fs.writeFileSync(path.join(tasksDir, '.gitkeep'), '');
    console.log('✅ Created tasks/ directory');
  } else {
    console.log('✅ tasks/ directory already exists');
  }

  // Make hooks executable
  const hooksDir = path.join(cwd, '.claude', 'hooks');
  if (fs.existsSync(hooksDir)) {
    try {
      for (const file of fs.readdirSync(hooksDir)) {
        const filePath = path.join(hooksDir, file);
        fs.chmodSync(filePath, '755');
      }
      console.log('✅ Made hooks executable');
    } catch (e) {
      console.log('⚠️  Could not set hook permissions (may need manual chmod)');
    }
  }

  // Summary
  console.log('');
  console.log('┌─────────────────────────────────────────────────────────────┐');
  console.log('│ 🎉 Setup Complete!                                          │');
  console.log('├─────────────────────────────────────────────────────────────┤');
  console.log('│ Created:                                                    │');
  console.log('│   • BACKLOG.md with EPIC-001 and FEAT-001-A                │');
  console.log('│   • tasks/ directory for detail files                      │');
  console.log('├─────────────────────────────────────────────────────────────┤');
  console.log('│ Next steps:                                                 │');
  console.log('│   1. Add tasks:  /task add "description" --feature FEAT-001-A');
  console.log('│   2. Start work: /task start TASK-001-A-1                  │');
  console.log('│   3. Complete:   /task done TASK-001-A-1                   │');
  console.log('└─────────────────────────────────────────────────────────────┘');
  console.log('');

  rl.close();
}

main().catch((error) => {
  console.error('Error:', error.message);
  process.exit(1);
});
