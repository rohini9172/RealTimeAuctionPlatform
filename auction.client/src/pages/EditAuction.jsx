import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  getAuctionById,
  toUtcAuctionDateTime,
  updateAuction,
} from '../services/auctionService';
import { getCategories } from '../services/categoryService';
import { useAuth } from '../context/AuthContext';
import AuctionForm from '../components/auctions/AuctionForm';
import Loading from '../components/common/Loading';
import ErrorMessage from '../components/common/ErrorMessage';

const EditAuction = () => {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [auction, setAuction] = useState(null);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitLoading, setSubmitLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadAuction = async () => {
      try {
        const [auctionData, categoryData] = await Promise.all([
          getAuctionById(Number(id)),
          getCategories(),
        ]);
        if (auctionData.status !== 'Scheduled') {
          setError('Only scheduled auctions can be edited.');
          return;
        }
        setAuction(auctionData);
        setCategories(categoryData);
      } catch (err) {
        setError(err.response?.data?.detail || 'Failed to load auction.');
      } finally {
        setLoading(false);
      }
    };

    loadAuction();
  }, [id]);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setSubmitLoading(true);
    try {
      const formData = new FormData(event.currentTarget);
      await updateAuction(Number(id), {
        title: formData.get('title'),
        description: formData.get('description'),
        categoryId: Number(formData.get('categoryId')),
        startingPrice: Number(formData.get('startingPrice')),
        minIncrement: Number(formData.get('minIncrement')),
        startTime: toUtcAuctionDateTime(formData.get('startTime')),
        endTime: toUtcAuctionDateTime(formData.get('endTime')),
        imageUrls: auction.imageUrls,
      });
      navigate('/my-auctions');
    } catch (err) {
      setError(
        err.response?.data?.detail ||
          Object.values(err.response?.data?.errors || {}).flat().join(' ') ||
          'Failed to update auction.'
      );
    } finally {
      setSubmitLoading(false);
    }
  };

  if (!user || user.role !== 'Seller') {
    return <div className="text-center py-5">Access denied. Seller role required.</div>;
  }

  if (loading) return <Loading />;

  return (
    <div className="container mt-4">
      {error && <ErrorMessage message={error} />}
      {auction && (
        <AuctionForm
          categories={categories}
          auction={auction}
          onSubmit={handleSubmit}
          loading={submitLoading}
          isEdit
        />
      )}
    </div>
  );
};

export default EditAuction;
