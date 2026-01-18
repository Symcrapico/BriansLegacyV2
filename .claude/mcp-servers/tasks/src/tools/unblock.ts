// ============================================================================
// unblock.ts
// Purpose: MCP tool to unblock a task and set it back to ready
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog } from '../writer.js';
import { findTask } from '../utils.js';
import { STATUS, ToolResult, textResult, errorResult } from '../types.js';

export const unblockTool = {
  schema: {
    description: 'Unblock a task and set it back to ready status',
    inputSchema: z.object({
      id: z.string().describe('Task ID to unblock'),
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
        return errorResult(`Task ${input.id} not found.`);
      }

      if (task.status !== STATUS.BLOCKED) {
        return errorResult(
          `Task ${input.id} is not blocked. Current status: ${task.status}`
        );
      }

      // Update status
      task.status = STATUS.READY;
      task.blockedReason = undefined;

      writeBacklog(backlog);

      return textResult(
        `🟦 Unblocked ${input.id}: ${task.description}\n` +
          `Status set to: ready`
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
