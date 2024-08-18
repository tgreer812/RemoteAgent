import sqlite3 from 'sqlite3';
import { open } from 'sqlite';
import dotenv from 'dotenv';
import log from '../utils/logger.js'

import * as userModel from '../models/userModel.js';
import * as agentModel from '../models/agentModel.js'

dotenv.config();

async function initializeDb() {
    let database_file;
    let reset_database = false;

    if (process.env.IS_DEBUG === 'true') {
        log.info("Initializing test database")
        database_file = process.env.DB_FILE_TEST || './database_test.db';
        reset_database = true;
    }
    else {
        database_file = process.env.DB_FILE || './database.db';
    }

    const db = await open({
        filename: database_file,
        driver: sqlite3.Database,
    });

    if (reset_database) {

        await dropTables(db);
    }

    await createTables(db);

    return db;
}

async function closeDb(db) {
    await db.close();
}

async function createTables(db) {
    log.info("Creating all tables");
    await userModel.createTable(db);
    await agentModel.createTable(db);    
}

async function dropTables(db) {
    log.warn("Dropping all tables!");
    await userModel.dropTable(db);
    await agentModel.dropTable(db);
}

export { initializeDb };
