import React, { useState } from 'react';
import VerticalNavbar from './components/VerticalNavbar'; // Import the navbar component
import './App.css';

import Agents from './pages/Agents'
import Plugins from './pages/Plugins'
import Settings from './pages/Settings'

function App() {
  const [currentView, setCurrentView] = useState('');

  const handleMenuClick = (item) => {
    let curView;
    console.log(item);
    switch (item.label) {
      case "Agents":
        curView = <Agents></Agents>
        break;
      case 'Plugins':
        curView = <Plugins></Plugins>;
        break;
      case 'Settings':
        curView = <Settings></Settings>;
        break;
      default:
        curView = (<h1>test</h1>);
    }
    setCurrentView(curView);
  };

  const navbarItems = [
    { label: 'Agents' },
    { label: 'Plugins' },
    { label: 'Settings' },
    { label: 'Log Out' }
  ];

  return (
    <div className="grid-container">
      <VerticalNavbar items={navbarItems} onItemClick={handleMenuClick} />
      <div className="current-view">
        {currentView}
      </div>
    </div>
  );
}

export default App;
