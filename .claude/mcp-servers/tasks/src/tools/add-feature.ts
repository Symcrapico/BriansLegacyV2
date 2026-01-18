// ============================================================================
// add-feature.ts
// Purpose: MCP tool to create a new feature under an epic
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog } from '../writer.js';
import { findEpic, findFeature, generateFeatureId } from '../utils.js';
import { Feature, ToolResult, textResult, errorResult } from '../types.js';

export const addFeatureTool = {
  schema: {
    description: 'Create a new feature under an epic',
    inputSchema: z.object({
      title: z.string().describe('Feature title'),
      epic: z.string().describe('Parent epic ID (e.g., EPIC-001)'),
      id: z
        .string()
        .optional()
        .describe('Feature ID (auto-generated if not provided)'),
    }),
  },

  handler: async (input: {
    title: string;
    epic: string;
    id?: string;
  }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();

      // Find parent epic
      const epic = findEpic(backlog, input.epic);
      if (!epic) {
        return errorResult(
          `Epic ${input.epic} not found. Create it first with add_epic.`
        );
      }

      // Generate or validate ID
      const featureId = input.id || generateFeatureId(epic);

      // Check for duplicate
      if (findFeature(backlog, featureId)) {
        return errorResult(`Feature ${featureId} already exists.`);
      }

      const feature: Feature = {
        id: featureId,
        title: input.title,
        epicId: epic.id,
        tasks: [],
        completedCount: 0,
        totalCount: 0,
      };

      epic.features.push(feature);
      writeBacklog(backlog);

      return textResult(
        `Created ${featureId}: ${input.title}\n` +
          `Under: ${epic.id} (${epic.title})\n` +
          `Next: Add tasks with create`
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};
