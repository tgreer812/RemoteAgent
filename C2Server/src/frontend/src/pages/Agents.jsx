import React, { useState, useEffect } from 'react';
import AgentCard from './AgentCard'; // Assuming you have an AgentCard component
import { getAgents } from '../utils/apis/backend/agent.js'; // Import the getAgents function

function Agents() {
    const [loadedAgents, setLoadedAgents] = useState([]);

    useEffect(() => {
        getLoadedAgents();
    }, []);

    async function getLoadedAgents() {
        try {
            const response = await getAgents();
            if (response && response.agents) {
                setLoadedAgents(response.agents);
            } else {
                setLoadedAgents([]); // Handle case where no agents are found
            }
        } catch (error) {
            console.error("Failed to fetch agents:", error);
            setLoadedAgents([]); // Handle error case
        }
    }

    return (
        <>
            <h1>Agents</h1>
            <div className="grid-container">
                {loadedAgents.length > 0 ? (
                    loadedAgents.map((agent, index) => (
                        <AgentCard
                            key={index}
                            id={agent.id}
                            name={agent.name}
                            os={agent.os}
                            ip={agent.ip}
                            status={agent.status}
                            onClickCallback={() => console.log(`Agent ${agent.name} clicked!`)}
                        />
                    ))
                ) : (
                    <p>No agents found</p>
                )}
            </div>
        </>
    );
}

export default Agents;
