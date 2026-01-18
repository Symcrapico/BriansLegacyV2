// ============================================================================
// add-epic.ts
// Purpose: MCP tool to create a new epic
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog } from '../writer.js';
import { generateEpicId, findEpic } from '../utils.js';
import { Epic, ToolResult, textResult, errorResult } from '../types.js';

export const addEpicTool = {
  schema: {
    description: 'Create a new epic',
    inputSchema: z.object({
      title: z.string().describe('Epic title'),
      id: z
        .string()
        .optional()
        .describe('Epic ID (auto-generated if not provided)'),
      priority: z
        .enum(['P0', 'P1', 'P2', 'P3'])
        .default('P1')
        .describe('Priority (default: P1)'),
    }),
  },

  handler: async (input: {
    title: string;
    id?: string;
    priority?: 'P0' | 'P1' | 'P2' | 'P3';
  }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();

      // Generate or validate ID
      const epicId = input.id || generateEpicId(backlog);

      // Check for duplicate
      if (findEpic(backlog, epicId)) {
        return errorResult(`Epic ${epicId} already exists.`);
      }

      const epic: Epic = {
        id: epicId,
        title: input.title,
        priority: input.priority || 'P1',
        features: [],
        completedCount: 0,
        totalCount: 0,
      };

      backlog.epics.push(epic);
      writeBacklog(backlog);

      return textResult(
        `Created ${epicId}: ${input.title}\n` +
          `Priority: ${epic.priority}\n` +
          `Next: Add features with add_feature`
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
