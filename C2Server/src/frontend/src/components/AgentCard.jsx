import React from 'react';
import '../styles/AgentCard.css';

function AgentCard({ id, name, os, ip, status, onClickCallback }) {
  return (
    <div className="agent-card" onClick={ onClickCallback }>
        <h1>{name}</h1>
        <p>ID: {id}</p>
        <p>OS: {os}</p>
        <p>IP: {ip}</p>
        <p>Status: {status}</p>
    </div>
  );
}

export default AgentCard;