export async function createAgentTable(db) {
    await db.exec(`
        CREATE TABLE IF NOT EXISTS agents (
            uuid TEXT PRIMARY KEY,
            ipv4 TEXT
        );
    `);
}