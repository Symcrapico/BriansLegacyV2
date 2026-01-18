// ============================================================================
// parser.ts
// Purpose: Parse BACKLOG.md into structured data with strict validation
// Dependencies: fs, path, types
// ============================================================================

import * as fs from 'fs';
import * as path from 'path';
import {
  Backlog,
  Epic,
  Feature,
  Task,
  CompletedTask,
  STATUS,
  StatusType,
  STATUS_FROM_EMOJI,
} from './types.js';

export function getBacklogPath(): string {
  return path.join(process.cwd(), 'docs', 'BACKLOG.md');
}

export class BacklogParser {
  private lines: string[] = [];
  private currentLine: number = 0;
  private filePath: string;

  constructor(filePath?: string) {
    this.filePath = filePath || getBacklogPath();
  }

  /**
   * Parse BACKLOG.md and return structured data
   * Throws on parse errors (strict mode)
   */
  parse(): Backlog {
    if (!fs.existsSync(this.filePath)) {
      throw new Error(
        `BACKLOG.md not found at ${this.filePath}. Run '/task init' to initialize.`
      );
    }

    const content = fs.readFileSync(this.filePath, 'utf-8');
    // Normalize line endings (handle both CRLF and LF)
    this.lines = content.replace(/\r\n/g, '\n').split('\n');
    this.currentLine = 0;

    const backlog: Backlog = {
      updated: new Date().toISOString(),
      active: null,
      epics: [],
      completed: [],
    };

    // Parse header metadata
    this.parseMetadata(backlog);

    // Parse epics and features
    while (this.currentLine < this.lines.length) {
      const line = this.lines[this.currentLine];

      if (line.startsWith('## EPIC-')) {
        backlog.epics.push(this.parseEpic());
      } else if (line.startsWith('## Completed')) {
        backlog.completed = this.parseCompletedSection();
        break;
      } else {
        this.currentLine++;
      }
    }

    return backlog;
  }

  private parseMetadata(backlog: Backlog): void {
    for (; this.currentLine < this.lines.length; this.currentLine++) {
      const line = this.lines[this.currentLine];

      const updatedMatch = line.match(/^> Updated: (.+)$/);
      if (updatedMatch) {
        backlog.updated = updatedMatch[1];
        continue;
      }

      const activeMatch = line.match(/^> Active: (TASK-[\w-]+|None)$/);
      if (activeMatch) {
        backlog.active = activeMatch[1] === 'None' ? null : activeMatch[1];
        continue;
      }

      if (line.startsWith('## ')) {
        break; // Start of content
      }
    }
  }

  private parseEpic(): Epic {
    const headerLine = this.lines[this.currentLine];
    const headerMatch = headerLine.match(/^## (EPIC-\d+): (.+)$/);

    if (!headerMatch) {
      this.error(`Invalid epic header: ${headerLine}`);
    }

    const epic: Epic = {
      id: headerMatch![1],
      title: headerMatch![2],
      priority: 'P2',
      features: [],
      completedCount: 0,
      totalCount: 0,
    };

    this.currentLine++;

    // Parse priority line (optional)
    while (this.currentLine < this.lines.length) {
      const line = this.lines[this.currentLine];

      if (line.startsWith('**Priority:**')) {
        const priorityMatch = line.match(/\*\*Priority:\*\* (P\d)/);
        if (priorityMatch) {
          epic.priority = priorityMatch[1];
        }
        this.currentLine++;
        continue;
      }

      if (line.startsWith('### FEAT-')) {
        const feature = this.parseFeature(epic.id);
        epic.features.push(feature);
        epic.completedCount += feature.completedCount;
        epic.totalCount += feature.totalCount;
        continue;
      }

      if (line.startsWith('## ') || line.startsWith('---')) {
        break; // Next epic or section separator
      }

      this.currentLine++;
    }

    return epic;
  }

  private parseFeature(epicId: string): Feature {
    const headerLine = this.lines[this.currentLine];
    const headerMatch = headerLine.match(
      /^### (FEAT-[\w-]+): (.+?)(?: \((\d+)\/(\d+)\))?$/
    );

    if (!headerMatch) {
      this.error(`Invalid feature header: ${headerLine}`);
    }

    const feature: Feature = {
      id: headerMatch![1],
      title: headerMatch![2],
      epicId,
      tasks: [],
      completedCount: parseInt(headerMatch![3] || '0'),
      totalCount: parseInt(headerMatch![4] || '0'),
    };

    this.currentLine++;

    // Skip to table content
    while (this.currentLine < this.lines.length) {
      const line = this.lines[this.currentLine];

      // Skip table header
      if (line.startsWith('| ID') || line.startsWith('|---')) {
        this.currentLine++;
        continue;
      }

      // Parse task row
      if (line.startsWith('| TASK-')) {
        const task = this.parseTaskRow(feature.id, epicId);
        if (task) {
          feature.tasks.push(task);
        }
        continue;
      }

      // End of feature
      if (
        line.startsWith('### ') ||
        line.startsWith('## ') ||
        line.startsWith('---')
      ) {
        break;
      }

      this.currentLine++;
    }

    // Recalculate counts from actual tasks
    feature.totalCount = feature.tasks.length;
    feature.completedCount = feature.tasks.filter(
      (t) => t.status === STATUS.DONE
    ).length;

    return feature;
  }

  private parseTaskRow(featureId: string, epicId: string): Task | null {
    const line = this.lines[this.currentLine];
    // | TASK-001-A-1 | Create EmailTemplates folder | 🟦 | 30m |
    const match = line.match(
      /^\| (TASK-[\w-]+) \| (.+?) \| ([⬜🟦🟨🟥✅]) \| (\d+m)? ?\|$/
    );

    this.currentLine++;

    if (!match) {
      // Try looser matching for manual edits
      const looseMatch = line.match(/^\| (TASK-[\w-]+) \| (.+?) \|/);
      if (looseMatch) {
        // Extract status from anywhere in the line
        let status: StatusType = STATUS.READY;
        for (const [emoji, _] of Object.entries(STATUS_FROM_EMOJI)) {
          if (line.includes(emoji)) {
            status = emoji as StatusType;
            break;
          }
        }
        return {
          id: looseMatch[1],
          description: looseMatch[2].trim(),
          status,
          featureId,
          epicId,
        };
      }
      return null;
    }

    return {
      id: match[1],
      description: match[2].trim(),
      status: match[3] as StatusType,
      estimate: match[4] || undefined,
      featureId,
      epicId,
    };
  }

  private parseCompletedSection(): CompletedTask[] {
    const completed: CompletedTask[] = [];
    this.currentLine++; // Skip "## Completed" header

    while (this.currentLine < this.lines.length) {
      const line = this.lines[this.currentLine];

      // Skip table header
      if (line.startsWith('| ID') || line.startsWith('|---')) {
        this.currentLine++;
        continue;
      }

      if (line.startsWith('| TASK-')) {
        // | TASK-001-A-1 | Create folder | 2025-12-29 10:00 | abc1234 |
        const match = line.match(
          /^\| (TASK-[\w-]+) \| (.+?) \| (.+?) \| (\w*)? ?\|$/
        );
        if (match) {
          completed.push({
            id: match[1],
            description: match[2].trim(),
            completed: match[3].trim(),
            commit: match[4]?.trim() || undefined,
          });
        }
      }

      this.currentLine++;
    }

    return completed;
  }

  private error(message: string): never {
    throw new Error(`Parse error at line ${this.currentLine + 1}: ${message}`);
  }
}

export function parseBacklog(filePath?: string): Backlog {
  const parser = new BacklogParser(filePath);
  return parser.parse();
}

export function backlogExists(filePath?: string): boolean {
  const path = filePath || getBacklogPath();
  return fs.existsSync(path);
}
