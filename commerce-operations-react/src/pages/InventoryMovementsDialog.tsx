import { Dialog, DialogContent, DialogTitle, Table, TableBody, TableCell, TableHead, TableRow } from '@mui/material';
import type { Inventory, InventoryMovement } from '../api/inventories';
interface Props { open: boolean; inventory: Inventory | null; movements: InventoryMovement[]; onClose: () => void }
export function InventoryMovementsDialog({ open, inventory, movements, onClose }: Props) {
  return <Dialog open={open} onClose={onClose} fullWidth maxWidth="md"><DialogTitle>재고 이력 · {inventory?.sku}</DialogTitle><DialogContent><Table size="small"><TableHead><TableRow><TableCell>시각</TableCell><TableCell>유형</TableCell><TableCell align="right">증감</TableCell><TableCell align="right">조정 후</TableCell><TableCell>사유</TableCell></TableRow></TableHead><TableBody>{movements.map((movement) => <TableRow key={movement.id}><TableCell>{new Date(movement.createdAt).toLocaleString('ko-KR')}</TableCell><TableCell>{movement.movementType}</TableCell><TableCell align="right">{movement.quantityDelta > 0 ? `+${movement.quantityDelta}` : movement.quantityDelta}</TableCell><TableCell align="right">{movement.availableAfter}</TableCell><TableCell>{movement.reason}</TableCell></TableRow>)}</TableBody></Table></DialogContent></Dialog>;
}

