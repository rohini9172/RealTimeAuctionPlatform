import React from 'react';
import { Link } from 'react-router-dom';

const NotFound = () => (
  <div className="container mt-5">
    <div className="row justify-content-center">
      <div className="col-md-6 text-center">
        <h1 className="display-1 fw-bold">404</h1>
        <p className="lead">Page not found</p>
        <Link to="/" className="btn btn-primary">
          Go to Home
        </Link>
      </div>
    </div>
  </div>
);

export default NotFound;
