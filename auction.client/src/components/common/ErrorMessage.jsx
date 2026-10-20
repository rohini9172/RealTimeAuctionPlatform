import React from 'react';

const ErrorMessage = ({ message }) => (
  <div
    style={{
      backgroundColor: '#f8d7da',
      color: '#721c24',
      padding: '12px',
      borderRadius: '4px',
      border: '1px solid #f5c6cb',
      marginBottom: '16px',
    }}
  >
    {message}
  </div>
);

export default ErrorMessage;
