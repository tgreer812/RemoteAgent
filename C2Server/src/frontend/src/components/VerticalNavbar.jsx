import React from 'react';

function VerticalNavbar({ items, onItemClick }) {
  return (
    <div className="vertical-navbar">
      <ul className="vertical-navbar-items">
        {items.map((item, index) => (
          <li
            key={index}
            className="vertical-navbar-item"
            onClick={() => onItemClick(item)}
          >
            {item.label}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default VerticalNavbar;
