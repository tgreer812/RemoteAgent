import sqlite3 from 'sqlite3';
import { open } from 'sqlite';
import dotenv from 'dotenv';

import * as userModel from '../models/userModel.js';
import * as agentModel from '../models/agentModel.js'

dotenv.config();

async function initializeDb() {
    let database_file;

    if (process.env.IS_DEBUG === 'true') {
        database_file = process.env.DB_FILE_TEST || './database_test.db';
    }
    else {
        database_file = process.env.DB_FILE || './database.db';
    }

    const db = await open({
        filename: database_file,
        driver: sqlite3.Database,
    });

    await createTables(db);

    return db;
}

async function closeDb(db) {
    await db.close();
}

async function createTables(db) {
    await userModel.createUserTable(db);
    await agentModel.createAgentTable(db);    
}

export { initializeDb };
