import express from 'express';

// print out cwd
console.log(process.cwd());
import apiRoutes from './routes/apiRoutes.js';
import dotenv from 'dotenv';
import { initializeDb } from './config/db.js';

dotenv.config();

const app = express();
const port = process.env.PORT || 3000;

initializeDb().then(() => { console.log('Database initialized'); });

app.use(express.json());

app.get('/', (req, res) => {
  res.send('Hello World!');
});

app.use('/api', apiRoutes);

app.listen(port, () => {
  console.log(`Server is running on http://localhost:${port}`);
});
