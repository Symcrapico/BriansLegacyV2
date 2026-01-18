// ============================================================================
// start.ts
// Purpose: MCP tool to start working on a task
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog, createTaskDetailFile } from '../writer.js';
import { findTask, getActiveTask, getNextReadyTasks } from '../utils.js';
import { STATUS, ToolResult, textResult, errorResult } from '../types.js';

export const startTool = {
  schema: {
    description:
      'Start working on a task. Sets it as active and creates a detail file. If no ID provided, starts the next ready task.',
    inputSchema: z.object({
      id: z.string().optional().describe('Task ID (e.g., TASK-001-A-3). If omitted, starts the next ready task.'),
    }),
  },

  handler: async (input: { id?: string }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();

      // Determine which task to start
      let taskId = input.id;

      if (!taskId) {
        // Auto-select the next ready task
        const nextTasks = getNextReadyTasks(backlog, 1);
        if (nextTasks.length === 0) {
          return errorResult('No ready tasks available. Add tasks or mark existing ones as ready.');
        }
        taskId = nextTasks[0].id;
      }

      // Check if another task is active
      const currentActive = getActiveTask(backlog);
      if (currentActive && currentActive.id !== taskId) {
        return errorResult(
          `${currentActive.id} is already active.\n` +
            `Complete it with: mcp__tasks__complete {id: "${currentActive.id}"}\n` +
            `Or block it with: mcp__tasks__block {id: "${currentActive.id}", reason: "..."}`
        );
      }

      // Find the task
      const task = findTask(backlog, taskId);
      if (!task) {
        return errorResult(`Task ${taskId} not found.`);
      }

      // Check if task is already completed
      if (task.status === STATUS.DONE) {
        return errorResult(`Task ${taskId} is already completed.`);
      }

      // Update task status
      task.status = STATUS.IN_PROGRESS;
      task.started = new Date().toISOString();
      backlog.active = taskId;

      // Write changes
      writeBacklog(backlog);
      const detailPath = createTaskDetailFile(task);

      const autoSelected = !input.id ? ' (auto-selected)' : '';
      return textResult(
        `Started ${taskId}${autoSelected}: ${task.description}\n` +
          `Detail file created: ${detailPath}\n` +
          `Estimate: ${task.estimate || 'Not set'}`
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
