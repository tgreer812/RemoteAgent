import express from 'express';
import dotenv from 'dotenv';
import apiRoutes from './routes/apiRoutes.js';
import { initializeDb } from './config/db.js';
import log from './utils/logger.js'

dotenv.config();

const app = express();
const port = process.env.PORT || 3000;

initializeDb().then(() => { log.info('Database initialized'); });

app.use(express.json());

app.get('/', (req, res) => {
  res.send('Hello World!');
});

app.use('/api', apiRoutes);

app.listen(port, () => {
  log.info(`Server is running on http://localhost:${port}`);
});
