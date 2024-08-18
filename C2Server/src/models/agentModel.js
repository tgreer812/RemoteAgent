export async function createTable(db) {
    await db.exec(`
        CREATE TABLE IF NOT EXISTS agents (
            uuid TEXT PRIMARY KEY,
            ipv4 TEXT
        );
    `);
}

export async function dropTable(db) {
    await db.exec("DROP TABLE IF EXISTS agents");
}