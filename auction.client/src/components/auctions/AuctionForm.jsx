import React from 'react';
import { formatAuctionDateTimeLocal, getAuctionImageUrl } from '../../services/auctionService';

const AuctionForm = ({
  categories,
  auction,
  onSubmit,
  loading,
  error,
  isEdit = false,
}) => {
  return (
    <div className="card mt-4">
      <div className="card-body">
        <h5 className="card-title mb-4">
          {isEdit ? 'Edit Auction' : 'Create Auction'}
        </h5>
        {error && <div className="alert alert-danger">{error}</div>}
        <form onSubmit={(e) => onSubmit(e)}>
          <div className="mb-3">
            <label className="form-label">Title</label>
            <input
              type="text"
              className="form-control"
              name="title"
              defaultValue={auction?.title || ''}
              required
            />
          </div>
          <div className="mb-3">
            <label className="form-label">Description</label>
            <textarea
              className="form-control"
              name="description"
              defaultValue={auction?.description || ''}
              rows={3}
              required
            />
          </div>
          <div className="mb-3">
            <label className="form-label">Category</label>
            <select
              className="form-select"
              name="categoryId"
              defaultValue={auction?.categoryId || ''}
              required
            >
              <option value="">Select a category</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>
          <div className="mb-3">
            <label className="form-label">Starting Price</label>
            <input
              type="number"
              className="form-control"
              name="startingPrice"
              step="0.01"
              defaultValue={auction?.startingPrice || ''}
              required
            />
          </div>
          <div className="mb-3">
            <label className="form-label">Minimum Increment</label>
            <input
              type="number"
              className="form-control"
              name="minIncrement"
              step="0.01"
              defaultValue={auction?.minIncrement || ''}
              required
            />
          </div>
          <div className="mb-3">
            <label className="form-label">Start Time</label>
            <input
              type="datetime-local"
              className="form-control"
              name="startTime"
              defaultValue={formatAuctionDateTimeLocal(auction?.startTime)}
              required
            />
          </div>
          <div className="mb-3">
            <label className="form-label">End Time</label>
            <input
              type="datetime-local"
              className="form-control"
              name="endTime"
              defaultValue={formatAuctionDateTimeLocal(auction?.endTime)}
              required
            />
          </div>
          {!isEdit && (
            <div className="mb-3">
              <label className="form-label" htmlFor="auction-images">
                Auction Images
              </label>
              <input
                id="auction-images"
                type="file"
                className="form-control"
                name="images"
                accept="image/jpeg,image/png,image/webp"
                multiple
                required
              />
              <div className="form-text">
                Upload 1-5 JPEG, PNG, or WebP images. Each image must be 5 MB or smaller.
              </div>
            </div>
          )}
          {isEdit && auction?.imageUrls?.length > 0 && (
            <div className="mb-3">
              <span className="form-label d-block">Current images</span>
              <div className="d-flex flex-wrap gap-2">
                {auction.imageUrls.map((imageUrl) => (
                  <img
                    key={imageUrl}
                    src={getAuctionImageUrl(imageUrl)}
                    alt={auction.title}
                    style={{ width: '120px', height: '90px', objectFit: 'cover' }}
                    className="rounded"
                  />
                ))}
              </div>
              <div className="form-text">Existing images will be kept when you save changes.</div>
            </div>
          )}
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Saving...' : isEdit ? 'Update Auction' : 'Create Auction'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default AuctionForm;
