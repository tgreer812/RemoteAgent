import sqlite3 from 'sqlite3';
import { open } from 'sqlite';
import dotenv from 'dotenv';

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


    return db;
}

async function closeDb(db) {
    await db.close();
}

async function createTables(db) {
    await db.exec(`
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL
        );
    `);

    await db.exec(`
        CREATE TABLE IF NOT EXISTS agents (
            uuid TEXT PRIMARY KEY,
            ipv4 TEXT,
        );
    `)
}

export { initializeDb };
