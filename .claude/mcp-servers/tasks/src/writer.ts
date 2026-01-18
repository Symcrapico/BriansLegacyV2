// ============================================================================
// writer.ts
// Purpose: Write structured data back to BACKLOG.md and manage detail files
// Dependencies: fs, path, types
// ============================================================================

import * as fs from 'fs';
import * as path from 'path';
import { Backlog, Epic, Feature, Task, STATUS } from './types.js';
import { getBacklogPath } from './parser.js';

export function getTasksDir(): string {
  return path.join(process.cwd(), 'docs', 'tasks');
}

export class BacklogWriter {
  private filePath: string;
  private tasksDir: string;

  constructor(filePath?: string) {
    this.filePath = filePath || getBacklogPath();
    this.tasksDir = getTasksDir();
  }

  /**
   * Write backlog data to BACKLOG.md
   */
  write(backlog: Backlog): void {
    const lines: string[] = [];

    // Header
    lines.push('# Backlog');
    lines.push('');
    lines.push(`> Updated: ${new Date().toISOString()}`);
    lines.push(`> Active: ${backlog.active || 'None'}`);
    lines.push('');
    lines.push('---');
    lines.push('');

    // Epics
    for (const epic of backlog.epics) {
      lines.push(...this.writeEpic(epic));
    }

    // Completed section
    lines.push('---');
    lines.push('');
    lines.push('## Completed');
    lines.push('');
    if (backlog.completed.length > 0) {
      lines.push('| ID | Task | Completed | Commit |');
      lines.push('|----|------|-----------|--------|');
      for (const task of backlog.completed) {
        const commit = task.commit || '';
        lines.push(
          `| ${task.id} | ${task.description} | ${task.completed} | ${commit} |`
        );
      }
    } else {
      lines.push('*No completed tasks yet.*');
    }
    lines.push('');

    fs.writeFileSync(this.filePath, lines.join('\n'));
  }

  private writeEpic(epic: Epic): string[] {
    const lines: string[] = [];

    lines.push(`## ${epic.id}: ${epic.title}`);
    lines.push(
      `**Priority:** ${epic.priority} | **Progress:** ${epic.completedCount}/${epic.totalCount}`
    );
    lines.push('');

    for (const feature of epic.features) {
      lines.push(...this.writeFeature(feature));
    }

    return lines;
  }

  private writeFeature(feature: Feature): string[] {
    const lines: string[] = [];

    lines.push(
      `### ${feature.id}: ${feature.title} (${feature.completedCount}/${feature.totalCount})`
    );
    lines.push('');
    lines.push('| ID | Task | Status | Est |');
    lines.push('|----|------|--------|-----|');

    for (const task of feature.tasks) {
      const est = task.estimate || '';
      lines.push(
        `| ${task.id} | ${task.description} | ${task.status} | ${est} |`
      );
    }

    lines.push('');
    return lines;
  }

  /**
   * Create task detail file
   */
  createDetailFile(task: Task): string {
    if (!fs.existsSync(this.tasksDir)) {
      fs.mkdirSync(this.tasksDir, { recursive: true });
    }

    const filePath = path.join(this.tasksDir, `${task.id}.md`);
    const now = new Date().toISOString();

    const content = `# ${task.id}: ${task.description}

**Status:** ${task.status}
**Started:** ${task.started || now}
**Feature:** ${task.featureId}
**Epic:** ${task.epicId}
**Estimate:** ${task.estimate || 'Not set'}

## Notes

(Add implementation notes here)

## Acceptance Criteria

- [ ] Implementation complete
- [ ] Tests passing
- [ ] Code reviewed
`;

    fs.writeFileSync(filePath, content);
    return filePath;
  }

  /**
   * Rename detail file to completed prefix
   */
  markDetailFileCompleted(taskId: string): boolean {
    const originalPath = path.join(this.tasksDir, `${taskId}.md`);
    const completedPath = path.join(this.tasksDir, `_COMPLETED_${taskId}.md`);

    if (fs.existsSync(originalPath)) {
      fs.renameSync(originalPath, completedPath);
      return true;
    }
    return false;
  }

  /**
   * Check if detail file exists
   */
  detailFileExists(taskId: string): boolean {
    const filePath = path.join(this.tasksDir, `${taskId}.md`);
    return fs.existsSync(filePath);
  }
}

export function writeBacklog(backlog: Backlog, filePath?: string): void {
  const writer = new BacklogWriter(filePath);
  writer.write(backlog);
}

export function createTaskDetailFile(task: Task): string {
  const writer = new BacklogWriter();
  return writer.createDetailFile(task);
}

export function markTaskDetailCompleted(taskId: string): boolean {
  const writer = new BacklogWriter();
  return writer.markDetailFileCompleted(taskId);
}

export function ensureTasksDir(): void {
  const tasksDir = getTasksDir();
  if (!fs.existsSync(tasksDir)) {
    fs.mkdirSync(tasksDir, { recursive: true });
  }
}
