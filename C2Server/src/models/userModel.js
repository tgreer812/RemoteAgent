//import sqlite3 from 'sqlite3';

export async function createTable(db) {
    await db.exec(`
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL
        );
    `);
}

export async function dropTable(db) {
    await db.exec("DROP TABLE IF EXISTS users");
}