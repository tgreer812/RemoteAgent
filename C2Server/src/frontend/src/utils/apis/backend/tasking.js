// A backend api for tasking an agent

let uri = 'http://localhost:3000/api/agent/';

export const addAgentTask = async (id, task) => {
    const response = await fetch(`${uri}${id}/tasking`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(task)
    });
    return await response.json();
}

export const getAgentTasks = async (id) => {
    const response = await fetch(`${uri}${id}/tasking`);
    return await response.json();
}

export const getAgentTask = async (id, taskId) => {
    const response = await fetch(`${uri}${id}/tasking/${taskId}`);
    return await response.json();
}

export const updateAgentTask = async (id, taskId, task) => {
    const response = await fetch(`${uri}${id}/tasking/${taskId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(task)
    });
    return await response.json();
}

export const cancelAgentTask = async (id, taskId) => {
    const response = await fetch(`${uri}${id}/tasking/${taskId}`, {
        method: 'DELETE'
    });
    return await response.json();
}