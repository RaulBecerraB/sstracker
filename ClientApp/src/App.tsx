import React, { useState, useEffect } from "react";
import Calendar from "./components/Calendar";
import { api, convertApiToScheduleEntries } from "./api";
import {
  ScheduleEntry,
  AvailableAdvisorsResponse,
  HealthResponse,
} from "./types";
import "./App.css";

const App: React.FC = () => {
  const [scheduleData, setScheduleData] = useState<ScheduleEntry[]>([]);
  const [currentAdvisors, setCurrentAdvisors] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>("");
  const [healthInfo, setHealthInfo] = useState<HealthResponse | null>(null);

  // Fetch schedule data from API
  const fetchScheduleData = async () => {
    try {
      const response = await api.getCalendarioCompleto();
      const entries = convertApiToScheduleEntries(response);
      setScheduleData(entries);
      return entries;
    } catch (err) {
      throw new Error('Error al cargar los datos del calendario');
    }
  };

  const fetchCurrentAdvisors = async () => {
    try {
      const response: AvailableAdvisorsResponse =
        await api.getAvailableAdvisors();
        setCurrentAdvisors(response.available || []);
    } catch (err) {
      setCurrentAdvisors([]);
    }
  };

  const fetchHealthInfo = async () => {
    try {
      const response: HealthResponse = await api.getHealth();
      setHealthInfo(response);
    } catch (err) {
      setHealthInfo(null);
    }
  };

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      try {
        // Fetch schedule data from API
        await fetchScheduleData();
 
        // Fetch current advisors
        await fetchCurrentAdvisors();
 
        // Fetch health info
        await fetchHealthInfo();
 
        setError("");
      } catch (err) {
        setError(err instanceof Error ? err.message : "Error al cargar los datos del calendario");
      } finally {
        setLoading(false);
      }
    };

    loadData();

    // Refresh data every 30 seconds
    const interval = setInterval(async () => {
      try {
        await fetchCurrentAdvisors();
        await fetchScheduleData(); // Also refresh schedule data for real-time updates
      } catch (err) {
        // Silent fail for background refresh
      }
    }, 30000);
    
    return () => clearInterval(interval);
  }, []);

  const refreshData = async () => {
    try {
      await fetchScheduleData();
      await fetchCurrentAdvisors();
      await fetchHealthInfo();
    } catch (err) {
      // Handle error if needed
    }
  };

  if (loading) {
    return (
      <div className="app">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Cargando calendario...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="app">
        <div className="error-container">
          <h2>❌ Error</h2>
          <p>{error}</p>
          <button onClick={refreshData} className="retry-button">
            Reintentar
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <Calendar scheduleData={scheduleData} currentAdvisors={currentAdvisors} />

      <div className="app-footer">
        <p>
          Última actualización: {new Date().toLocaleTimeString("es-ES")} |
          Entradas en calendario: {scheduleData.length}
        </p>
      </div>
    </div>
  );
};

export default App;
