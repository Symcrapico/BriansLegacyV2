// ============================================================================
// delete.ts
// Purpose: MCP tool to delete (archive) a task
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog, markTaskDetailCompleted } from '../writer.js';
import { findTask, removeTask, recalculateCounts, formatTimestamp } from '../utils.js';
import { CompletedTask, ToolResult, textResult, errorResult } from '../types.js';

export const deleteTool = {
  schema: {
    description:
      'Delete (archive) a task. Moves it to completed with a cancelled note.',
    inputSchema: z.object({
      id: z.string().describe('Task ID to delete'),
      reason: z
        .string()
        .optional()
        .describe('Reason for deletion (optional)'),
    }),
  },

  handler: async (input: { id: string; reason?: string }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      const task = findTask(backlog, input.id);

      if (!task) {
        return errorResult(`Task ${input.id} not found.`);
      }

      // If this was the active task, clear it
      if (backlog.active === input.id) {
        backlog.active = null;
      }

      // Remove from feature
      removeTask(backlog, input.id);

      // Add to completed with cancelled marker
      const reason = input.reason ? ` (${input.reason})` : '';
      const completedTask: CompletedTask = {
        id: task.id,
        description: `[CANCELLED] ${task.description}${reason}`,
        completed: formatTimestamp(new Date().toISOString()),
      };
      backlog.completed.unshift(completedTask);

      recalculateCounts(backlog);
      writeBacklog(backlog);
      markTaskDetailCompleted(input.id);

      return textResult(
        `Deleted ${input.id}: ${task.description}\n` +
          (input.reason ? `Reason: ${input.reason}` : 'Moved to completed as cancelled.')
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
