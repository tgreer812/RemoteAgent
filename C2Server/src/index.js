import express from 'express';
import dotenv from 'dotenv';
import path from 'path';
import { fileURLToPath } from 'url';  // Import this to resolve __dirname
import apiRoutes from './routes/apiRoutes.js';
import { initializeDb } from './config/db.js';
import log from './utils/logger.js';

dotenv.config();

// Resolve __dirname equivalent in ES modules
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
const port = process.env.PORT || 3000;

// Initialize the database and log success/failure
initializeDb()
  .then(() => { log.info('Database initialized'); })
  .catch((error) => { log.error('Database initialization failed:', error); return; });

app.use(express.json()); // Middleware to parse JSON

// Serve static files from the React frontend
app.use(express.static(path.join(__dirname, 'frontend/dist')));

// API routes
app.use('/api', apiRoutes);

// Catch-all route for serving React frontend for non-API requests
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'frontend/dist/index.html'));
});

// Start the server
app.listen(port, () => {
  log.info(`Server is running on http://localhost:${port}`);
});
