// ============================================================================
// get.ts
// Purpose: MCP tool to get detailed information about a specific task
// Dependencies: parser, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { findTask, findFeature, findEpic, timeSince } from '../utils.js';
import { ToolResult, jsonResult, errorResult } from '../types.js';
import { BacklogWriter } from '../writer.js';

export const getTool = {
  schema: {
    description: 'Get detailed information about a specific task by ID',
    inputSchema: z.object({
      id: z.string().describe('Task ID (e.g., TASK-001-A-3)'),
    }),
  },

  handler: async (input: { id: string }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      const task = findTask(backlog, input.id);

      if (!task) {
        // Check completed tasks
        const completed = backlog.completed.find((t) => t.id === input.id);
        if (completed) {
          return jsonResult({
            ...completed,
            status: '✅',
            isCompleted: true,
          });
        }
        return errorResult(`Task ${input.id} not found.`);
      }

      const feature = findFeature(backlog, task.featureId);
      const epic = findEpic(backlog, task.epicId);
      const writer = new BacklogWriter();
      const hasDetailFile = writer.detailFileExists(input.id);

      return jsonResult({
        ...task,
        startedAgo: task.started ? timeSince(task.started) : null,
        feature: feature
          ? {
              id: feature.id,
              title: feature.title,
              progress: `${feature.completedCount}/${feature.totalCount}`,
            }
          : null,
        epic: epic
          ? {
              id: epic.id,
              title: epic.title,
              progress: `${epic.completedCount}/${epic.totalCount}`,
            }
          : null,
        hasDetailFile,
        detailFilePath: hasDetailFile ? `docs/tasks/${input.id}.md` : null,
      });
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
