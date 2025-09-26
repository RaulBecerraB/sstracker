import axios from "axios";
import {
  AvailableAdvisorsResponse,
  HealthResponse,
  ScheduleEntry,
} from "./types";

const API_BASE_URL = "/api/v1/schedule";
const HEALTH_URL = "/api/v1/health";

export const api = {
  getAvailableAdvisors: async (): Promise<AvailableAdvisorsResponse> => {
    const response = await axios.get(`${API_BASE_URL}/advisors/available`);
    return response.data;
  },

  getHealth: async (): Promise<HealthResponse> => {
    const response = await axios.get(HEALTH_URL);
    return response.data;
  },

  getCalendarioCompleto: async () => {
    const response = await axios.get(API_BASE_URL);
    return response.data;
  },
};

// Convert API response to ScheduleEntry format
export const convertApiToScheduleEntries = (
  apiResponse: any
): ScheduleEntry[] => {
  if (!apiResponse?.entries) return [];

  return apiResponse.entries.map((entry: any) => ({
    day: entry.day || "",
    start: entry.start || "",
    end: entry.end || "",
    advisors: entry.advisors || "",
  }));
};
