import React from 'react';

const SuccessMessage = ({ message }) => (
  <div
    style={{
      backgroundColor: '#d4edda',
      color: '#155724',
      padding: '12px',
      borderRadius: '4px',
      border: '1px solid #c3e6cb',
      marginBottom: '16px',
    }}
  >
    {message}
  </div>
);

export default SuccessMessage;
