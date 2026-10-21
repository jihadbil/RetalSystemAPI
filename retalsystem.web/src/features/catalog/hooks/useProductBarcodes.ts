import { useEffect, useState } from 'react';
import type { ProductDto } from '../types/catalog.types';
import { lookupService } from '../../../shared/services/lookupService';

export function useProductBarcodes(productId: string, enabled: boolean) {
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [barcodeId, setBarcodeId] = useState('');
  const [error, setError] = useState('');
  useEffect(() => {
    let cancelled = false;
    setProduct(null);
    setBarcodeId('');
    setError('');
    if (enabled && productId) {
      void lookupService.product(productId).then((details) => {
        if (cancelled) return;
        setProduct(details);
        if (details.barCodes?.length === 1) setBarcodeId(details.barCodes[0].id || '');
      }).catch(() => { if (!cancelled) setError('تعذر تحميل باركودات الصنف'); });
    }
    return () => { cancelled = true; };
  }, [productId, enabled]);
  return { product, barcodeId, setBarcodeId, error };
}
