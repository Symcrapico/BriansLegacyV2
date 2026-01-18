// ============================================================================
// complete.ts
// Purpose: MCP tool to complete a task
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog, markTaskDetailCompleted } from '../writer.js';
import {
  findTask,
  removeTask,
  recalculateCounts,
  formatTimestamp,
} from '../utils.js';
import { CompletedTask, ToolResult, textResult, errorResult } from '../types.js';

export const completeTool = {
  schema: {
    description: 'Complete a task. Moves it to the Completed section.',
    inputSchema: z.object({
      id: z.string().describe('Task ID to complete'),
      commit: z
        .string()
        .optional()
        .describe('Git commit hash (optional but recommended)'),
    }),
  },

  handler: async (input: { id: string; commit?: string }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();

      // Verify task exists
      const task = findTask(backlog, input.id);
      if (!task) {
        return errorResult(`Task ${input.id} not found.`);
      }

      // Warn if not the active task (but allow it)
      const wasActive = backlog.active === input.id;

      // Remove from feature's task list
      removeTask(backlog, input.id);

      // Add to completed section
      const now = new Date();
      const completedTask: CompletedTask = {
        id: task.id,
        description: task.description,
        completed: formatTimestamp(now.toISOString()),
        commit: input.commit,
      };
      backlog.completed.unshift(completedTask); // Add to top

      // Clear active if this was the active task
      if (wasActive) {
        backlog.active = null;
      }

      // Recalculate counts
      recalculateCounts(backlog);

      // Write changes
      writeBacklog(backlog);
      markTaskDetailCompleted(input.id);

      let message = `✅ Completed ${input.id}: ${task.description}`;
      if (input.commit) {
        message += `\nCommit: ${input.commit}`;
      } else {
        message += '\n⚠️ No commit hash provided. Consider adding one for traceability.';
      }
      if (!wasActive) {
        message += `\n⚠️ Note: This was not the active task.`;
      }

      return textResult(message);
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
