// ============================================================================
// block.ts
// Purpose: MCP tool to mark a task as blocked
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog } from '../writer.js';
import { findTask } from '../utils.js';
import { STATUS, ToolResult, textResult, errorResult } from '../types.js';

export const blockTool = {
  schema: {
    description: 'Mark a task as blocked with a reason',
    inputSchema: z.object({
      id: z.string().describe('Task ID to block'),
      reason: z.string().describe('Reason for blocking'),
    }),
  },

  handler: async (input: { id: string; reason: string }): Promise<ToolResult> => {
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

      // Update status
      const previousStatus = task.status;
      task.status = STATUS.BLOCKED;
      task.blockedReason = input.reason;

      writeBacklog(backlog);

      return textResult(
        `🟥 Blocked ${input.id}: ${task.description}\n` +
          `Reason: ${input.reason}\n` +
          `Previous status: ${previousStatus}`
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
