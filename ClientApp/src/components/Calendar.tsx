import React, { useState, useEffect } from "react";
import { ScheduleEntry } from "../types";
import "./Calendar.css";

interface CalendarProps {
  scheduleData: ScheduleEntry[];
  currentAdvisors: string[];
}

const Calendar: React.FC<CalendarProps> = ({
  scheduleData,
  currentAdvisors,
}) => {
  const [selectedDate, setSelectedDate] = useState<string>("");
  const [clickedCell, setClickedCell] = useState<string | null>(null);
  const [isMobile, setIsMobile] = useState<boolean>(false);

  // Working days in order (mapped from English to Spanish)
  const workingDaysMap = {
    Mon: "Lun",
    Tue: "Mar",
    Wed: "Mié",
    Thu: "Jue",
    Fri: "Vie",
  };
  const allWorkingDays = ["Mon", "Tue", "Wed", "Thu", "Fri"]; // Keep English for CSV compatibility
  const allWorkingDaysSpanish = ["Lun", "Mar", "Mié", "Jue", "Vie"]; // Spanish display

  // Check if it's mobile view
  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth <= 768);
    };
    
    checkMobile();
    window.addEventListener('resize', checkMobile);
    
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  // Get current day and next working day for mobile view
  const getCurrentAndNextDay = () => {
    const now = new Date();
    const currentDayIndex = now.getDay(); // 0=Sun, 1=Mon, 2=Tue, etc.
    
    // Map day index to working day name
    const dayIndexToName: Record<number, string> = {
      1: "Mon", // Monday
      2: "Tue", // Tuesday  
      3: "Wed", // Wednesday
      4: "Thu", // Thursday
      5: "Fri", // Friday
    };

    const currentDay = dayIndexToName[currentDayIndex];
    
    // If it's weekend or not a working day, show Monday and Tuesday
    if (!currentDay) {
      return ["Mon", "Tue"];
    }

    // If it's Friday, next working day is Monday
    if (currentDay === "Fri") {
      return ["Fri", "Mon"];
    }

    // For other days, show current and next day
    const currentIndex = allWorkingDays.indexOf(currentDay);
    const nextDay = allWorkingDays[currentIndex + 1];
    
    return [currentDay, nextDay];
  };

  // Determine which days to show based on screen size
  const [workingDays, workingDaysSpanish] = isMobile 
    ? (() => {
        const [currentDay, nextDay] = getCurrentAndNextDay();
        const currentIndex = allWorkingDays.indexOf(currentDay);
        const nextIndex = allWorkingDays.indexOf(nextDay);
        
        return [
          [currentDay, nextDay],
          [allWorkingDaysSpanish[currentIndex], allWorkingDaysSpanish[nextIndex]]
        ];
      })()
    : [allWorkingDays, allWorkingDaysSpanish];

  // Get all unique time slots from the schedule data
  const allTimeSlots = scheduleData
    .reduce((slots, entry) => {
      const startHour = parseInt(entry.start.split(":")[0]);
      const endHour = parseInt(entry.end.split(":")[0]);

      for (let hour = startHour; hour < endHour; hour++) {
        const timeSlot = `${hour.toString().padStart(2, "0")}:00`;
        if (!slots.includes(timeSlot)) {
          slots.push(timeSlot);
        }
      }
      return slots;
    }, [] as string[])
    .sort();

  // Create a grid structure: [timeSlot][day] = advisors[]
  const calendarGrid = allTimeSlots.reduce((grid, timeSlot) => {
    grid[timeSlot] = {};
    allWorkingDays.forEach((day) => {
      grid[timeSlot][day] = [];
    });
    return grid;
  }, {} as Record<string, Record<string, string[]>>);

  // Populate the grid with advisors
  scheduleData.forEach((entry) => {
    const startHour = parseInt(entry.start.split(":")[0]);
    const endHour = parseInt(entry.end.split(":")[0]);
    const advisors = entry.advisors
      .split(/[;,]/)
      .map((a) => a.trim())
      .filter((a) => a);

    for (let hour = startHour; hour < endHour; hour++) {
      const timeSlot = `${hour.toString().padStart(2, "0")}:00`;
      if (calendarGrid[timeSlot] && calendarGrid[timeSlot][entry.day]) {
        advisors.forEach((advisor) => {
          if (!calendarGrid[timeSlot][entry.day].includes(advisor)) {
            calendarGrid[timeSlot][entry.day].push(advisor);
          }
        });
      }
    }
  });

  const formatDayName = (dayAbbrev: string): string => {
    const dayNames: Record<string, string> = {
      Mon: "Lunes",
      Tue: "Martes",
      Wed: "Miércoles",
      Thu: "Jueves",
      Fri: "Viernes",
      Lun: "Lunes",
      Mar: "Martes",
      Mié: "Miércoles",
      Jue: "Jueves",
      Vie: "Viernes",
      Sat: "Sábado",
      Sun: "Domingo",
    };
    return dayNames[dayAbbrev] || dayAbbrev;
  };

  const isCurrentTimeSlot = (timeSlot: string, day: string): boolean => {
    const now = new Date();
    const currentDayIndex = now.getDay(); // 0=Sun, 1=Mon, 2=Tue, etc.
    const dayIndexMap: Record<string, number> = {
      Sun: 0,
      Mon: 1,
      Tue: 2,
      Wed: 3,
      Thu: 4,
      Fri: 5,
      Sat: 6,
    };
    const dayIndex = dayIndexMap[day];
    const currentHour = now.getHours();
    const slotHour = parseInt(timeSlot.split(":")[0]);

    return currentDayIndex === dayIndex && currentHour === slotHour;
  };

  // Color function removed - all advisors now use uniform colors

  const getAdvisorPhoto = (advisor: string): string => {
    const advisorPhotos: Record<string, string> = {
      Jorge: "/imgs/jorge.jpeg",
      Angel: "/imgs/angel.jpeg",
      Rafael: "/imgs/rafota.jpeg",
      Raul: "/imgs/raulete.jpeg",
    };
    return advisorPhotos[advisor] || "";
  };

  return (
    <div className="calendar-container">
      <div className="current-status">
        <div className="status-indicator">
          <span className="status-dot"></span>
          <span>Disponibles ahora: </span>
          {currentAdvisors.length > 0 ? (
            <span className="current-advisors">
              {currentAdvisors.join(", ")}
            </span>
          ) : (
            <span className="no-advisors">Nadie disponible</span>
          )}
        </div>
      </div>

      <div className="calendar-table">
        <div className={`calendar-grid ${isMobile ? 'mobile-grid' : 'desktop-grid'}`}>
          {/* Header row with days */}
          <div className="time-header"></div>
          {workingDays.map((day, index) => (
            <div key={day} className="day-header">
              <h3>{formatDayName(workingDaysSpanish[index])}</h3>
            </div>
          ))}

          {/* Time slots rows */}
          {allTimeSlots.map((timeSlot) => (
            <React.Fragment key={timeSlot}>
              <div className="time-label">
                <span className="time-text">{timeSlot}</span>
              </div>
              {workingDays.map((day) => {
                const cellId = `${timeSlot}-${day}`;
                const advisorsInCell = calendarGrid[timeSlot][day];
                return (
                  <div
                    key={cellId}
                    className={`calendar-cell ${
                      isCurrentTimeSlot(timeSlot, day) ? "current-time" : ""
                    } ${clickedCell === cellId ? "cell-clicked" : ""} ${
                      advisorsInCell.length === 0 ? "empty-cell" : ""
                    }`}
                    onClick={
                      advisorsInCell.length > 0
                        ? () =>
                            setClickedCell(
                              clickedCell === cellId ? null : cellId
                            )
                        : undefined
                    }
                  >
                    <div className="advisors-list">
                      {/* Always show names by default */}
                      <div className="advisor-names">
                        {advisorsInCell.map((advisor, idx) => (
                          <span
                            key={idx}
                            className={`advisor-tag ${
                              currentAdvisors.includes(advisor) &&
                              isCurrentTimeSlot(timeSlot, day)
                                ? "available-now"
                                : ""
                            }`}
                          >
                            {advisor}
                          </span>
                        ))}
                      </div>

                      {/* Photos that show on hover or click */}
                      <div className="advisor-photos">
                        {advisorsInCell.map(
                          (advisor, idx) =>
                            getAdvisorPhoto(advisor) && (
                              <img
                                key={idx}
                                src={getAdvisorPhoto(advisor)}
                                alt={advisor}
                                className="cell-advisor-photo"
                                title={advisor}
                              />
                            )
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
            </React.Fragment>
          ))}
        </div>
      </div>
    </div>
  );
};

export default Calendar;
