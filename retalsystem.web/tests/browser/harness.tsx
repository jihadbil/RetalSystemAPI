import { useState } from 'react';
import { createRoot } from 'react-dom/client';
import '../../src/index.css';
import { AuthProvider, useAuth } from '../../src/shared/context/AuthContext';
import { ToastProvider } from '../../src/shared/context/ToastContext';
import { Modal } from '../../src/shared/components/ui/Modal';
import { Select } from '../../src/shared/components/ui/Select';
import { DashboardView } from '../../src/features/dashboard/DashboardView';
import { StockView } from '../../src/features/stock/StockView';
import { SalesInvoiceDetailsModal } from '../../src/features/sales/components/SalesInvoiceDetailsModal';
import { PurchaseInvoiceDetailsModal } from '../../src/features/purchase/components/PurchaseInvoiceDetailsModal';
import { ExpenseFormModal } from '../../src/features/accounting/components/ExpenseFormModal';
import { PosTerminalFormModal } from '../../src/features/accounting/components/PosTerminalFormModal';
import { PosProvider, usePos } from '../../src/features/pos/context/PosContext';
import { PosPaymentPanel } from '../../src/features/pos/components/PosPaymentPanel';
import { PosSessionPaymentsView } from '../../src/features/accounting/components/PosSessionPaymentsView';

export function Dialogs() {
  const [parent, setParent] = useState(false);
  const [child, setChild] = useState(false);
  return <>
    <button onClick={() => setParent(true)}>Open parent</button>
    <Modal isOpen={parent} onClose={() => setParent(false)} title="Parent dialog">
      <button onClick={() => setChild(true)}>Open child</button>
      <button>Parent action</button>
      <Modal isOpen={child} onClose={() => setChild(false)} title="Child dialog">
        <input aria-label="Child input" /><button>Child action</button>
      </Modal>
    </Modal>
    <Modal isOpen={false} onClose={() => {}} title="Closed sibling">Closed</Modal>
  </>;
}
export function PosProbe() {
  const pos = usePos();
  return <>
    <button onClick={() => pos.addCartItem({ productId: 'product', productName: 'Product', code: 'p', barCode: 'bc', productBarCodeId: 'barcode', unitPrice: 10, quantity: 1, discountAmount: 0, lineTotal: 10 })}>Add item</button>
    <button onClick={() => pos.setSelectedWarehouseId('warehouse')}>Select warehouse</button>
    <button onClick={pos.setExactCash}>Exact cash</button>
    <button onClick={() => void pos.completeSale()}>Checkout</button>
    <button onClick={() => void pos.recoverPendingSale()}>Recover</button>
    <button onClick={() => void pos.closeCashSession(104)}>Close shift</button>
    <output data-testid="cart-count">{pos.cartItems.length}</output>
    <output data-testid="pending">{pos.pendingCheckout?.request.invoiceNumber || 'none'}</output>
    <output data-testid="receipt">{pos.lastReceipt?.invoiceNumber || 'none'}</output>
    <output data-testid="ready">{pos.deviceContext?.currentSession?.id || 'waiting'}</output>
  </>;
}
export function Harness() {
  const { switchBranch } = useAuth();
  const [invoiceId, setInvoiceId] = useState('invoice-a');
  const [selected, setSelected] = useState('');
  const mode = new URLSearchParams(location.search).get('mode');
  return <>
    <button onClick={() => switchBranch('branch-a', 'A')}>Branch A</button>
    <button onClick={() => switchBranch('branch-b', 'B')}>Branch B</button>
    <button onClick={() => setInvoiceId('invoice-a')}>Invoice A</button>
    <button onClick={() => setInvoiceId('invoice-b')}>Invoice B</button>
    {mode === 'dialogs' && <Dialogs />}
    {mode === 'dashboard' && <DashboardView onNavigate={() => {}} />}
    {mode === 'stock' && <StockView />}
    {mode === 'sales-details' && <SalesInvoiceDetailsModal isOpen onClose={() => {}} invoiceId={invoiceId} />}
    {mode === 'purchase-details' && <PurchaseInvoiceDetailsModal isOpen onClose={() => {}} invoiceId={invoiceId} />}
    {mode === 'lookup' && <Select lookup={{ resource: 'suppliers' }} label="Supplier" value={selected} onChange={(e) => setSelected(e.target.value)} />}
    {mode === 'expense' && <ExpenseFormModal isOpen onClose={() => {}} categories={[]} safes={[]} onSaved={() => {}} />}
    {mode === 'terminal' && <PosTerminalFormModal branchId="branch-a" onClose={() => {}} onSaved={() => {}} />}
    {mode === 'pos' && <PosProvider><PosProbe /></PosProvider>}
    {mode === 'pos-split' && <PosProvider><PosProbe /><PosPaymentPanel onOpenDraftsModal={() => {}} onOpenDiscountModal={() => {}} /></PosProvider>}
    {mode === 'session-payments' && <PosSessionPaymentsView />}
  </>;
}

createRoot(document.getElementById('root')!).render(<AuthProvider><ToastProvider><Harness /></ToastProvider></AuthProvider>);
