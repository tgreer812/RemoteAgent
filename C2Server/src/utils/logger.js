import winston from 'winston';
import dotenv from 'dotenv';

dotenv.config();

// Create the transports array (console is always included)
const transports = [
  new winston.transports.Console({
    format: winston.format.combine(
      winston.format.colorize(),    // Colorize console output
      winston.format.simple()       // Simple text output for console
    )
  })
];

// If LOG_FILE is set in .env, add File transport
if (process.env.LOG_FILE) {
  transports.push(
    new winston.transports.File({
      filename: process.env.LOG_FILE,
      format: winston.format.combine(
        winston.format.timestamp(),   // Add timestamp to log entries
        winston.format.json()         // JSON format for log file
      )
    })
  );
}

// Create the logger instance
const logger = winston.createLogger({
  level: process.env.LOG_LEVEL || 'info',   // Default log level
  transports: transports,
});

// Export the standard logging functions

const log = {
  info: (message) => logger.info(message),
  debug: (message) => logger.debug(message),
  warn: (message) => logger.warn(message),
  error: (message) => logger.error(message),
  exception: (error) => logger.error(`Exception: ${error.message}`, { stack: error.stack }),
};

export default log;
