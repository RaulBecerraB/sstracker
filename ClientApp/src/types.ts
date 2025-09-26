export interface ScheduleEntry {
  day: string;
  start: string;
  end: string;
  advisors: string;
}

export interface AvailableAdvisorsResponse {
  available: string[];
  message?: string;
}

export interface HealthResponse {
  status: string;
  serverTime: string;
  csvExists: boolean;
  csvLines?: number;
  csvLastModified?: string;
  error?: string;
}
