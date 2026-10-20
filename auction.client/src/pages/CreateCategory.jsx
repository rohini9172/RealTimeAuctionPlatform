import React, { useState } from 'react';
import { createCategory } from '../services/categoryService';

const CreateCategory = () => {
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      await createCategory(name.trim());
      setName('');
      setSuccess('Category created.');
    } catch (requestError) {
      setError(requestError.response?.data?.detail || 'Failed to create category.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="container mt-4">
      <h2>Create Category</h2>
      <form className="mt-3" onSubmit={handleSubmit}>
        <div className="mb-3">
          <label className="form-label" htmlFor="category-name">Category name</label>
          <input
            id="category-name"
            className="form-control"
            value={name}
            onChange={(event) => setName(event.target.value)}
            maxLength={100}
            required
          />
        </div>
        {error && <div className="alert alert-danger" role="alert">{error}</div>}
        {success && <div className="alert alert-success" role="status">{success}</div>}
        <button className="btn btn-primary" type="submit" disabled={loading}>
          {loading ? 'Creating...' : 'Create category'}
        </button>
      </form>
    </main>
  );
};

export default CreateCategory;