
// Implement CRUD operations for agents

let uri = 'http://localhost:3000/api/agents';

export const getAgents = async () => {
    const response = await fetch(uri);
    return await response.json();
};

export const getAgent = async (id) => {
    const response = await fetch(`${uri}/${id}`);
    return await response.json();
}

export const createAgent = async (agent) => {
    const response = await fetch(uri, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(agent)
    });
    return await response.json();
};

export const updateAgent = async (id, agent) => {
    const response = await fetch(`${uri}/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(agent)
    });
    return await response.json();
};

export const deleteAgent = async (id) => {
    const response = await fetch(`${uri}/${id}`, {
        method: 'DELETE'
    });
    return await response.json();
};