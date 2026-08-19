import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Alert,
  Box,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { getSettlements, rebuildSettlements } from "../api/settlements";
function iso(date: Date) {
  return date.toISOString().slice(0, 10);
}
const defaultTo = iso(new Date());
const defaultFrom = iso(new Date(Date.now() - 29 * 86400000));
export function SettlementsPage() {
  const [from, setFrom] = useState(defaultFrom);
  const [to, setTo] = useState(defaultTo);
  const qc = useQueryClient();
  const query = useQuery({
    queryKey: ["settlements", from, to],
    queryFn: () => getSettlements(from, to),
    enabled: !!from && !!to && from <= to,
  });
  const rebuild = useMutation({
    mutationFn: () => rebuildSettlements(from, to),
    onSuccess: () => void qc.invalidateQueries({ queryKey: ["settlements"] }),
  });
  const totals = query.data?.items.reduce(
    (a, x) => ({
      paid: a.paid + Number(x.paymentAmount),
      refunded: a.refunded + Number(x.refundAmount),
      net: a.net + Number(x.netAmount),
    }),
    { paid: 0, refunded: 0, net: 0 },
  );
  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "end",
          mb: 3,
          gap: 2,
          flexWrap: "wrap",
        }}
      >
        <Box>
          <Typography variant="h4">일별 매출 정산</Typography>
          <Typography color="text.secondary">
            UTC 기준 결제 승인과 환불 원장을 일자별로 집계합니다.
          </Typography>
        </Box>
        <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
          <TextField
            label="시작일"
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <TextField
            label="종료일"
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <Button
            variant="contained"
            disabled={!from || !to || from > to || rebuild.isPending}
            onClick={() => rebuild.mutate()}
          >
            재집계
          </Button>
        </Box>
      </Box>
      {from > to && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          시작일은 종료일보다 늦을 수 없습니다.
        </Alert>
      )}
      {(query.isError || rebuild.isError) && (
        <Alert severity="error" sx={{ mb: 2 }}>
          정산 요청을 처리하지 못했습니다.
        </Alert>
      )}
      <Box sx={{ display: "flex", gap: 3, mb: 2 }}>
        <Typography>승인 {totals?.paid.toLocaleString() ?? 0}원</Typography>
        <Typography>환불 {totals?.refunded.toLocaleString() ?? 0}원</Typography>
        <Typography sx={{ fontWeight: "bold" }}>
          순매출 {totals?.net.toLocaleString() ?? 0}원
        </Typography>
      </Box>
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>정산일</TableCell>
              <TableCell align="right">승인액</TableCell>
              <TableCell align="right">환불액</TableCell>
              <TableCell align="right">순매출</TableCell>
              <TableCell align="right">승인 건수</TableCell>
              <TableCell align="right">환불 건수</TableCell>
              <TableCell>계산 시각</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {query.data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7}>
                  재집계를 실행하면 선택 기간의 정산 행이 생성됩니다.
                </TableCell>
              </TableRow>
            ) : (
              query.data?.items.map((x) => (
                <TableRow key={x.settlementDate}>
                  <TableCell>{x.settlementDate}</TableCell>
                  <TableCell align="right">
                    {Number(x.paymentAmount).toLocaleString()}
                  </TableCell>
                  <TableCell align="right">
                    {Number(x.refundAmount).toLocaleString()}
                  </TableCell>
                  <TableCell align="right">
                    {Number(x.netAmount).toLocaleString()}
                  </TableCell>
                  <TableCell align="right">{x.paymentCount}</TableCell>
                  <TableCell align="right">{x.refundCount}</TableCell>
                  <TableCell>
                    {new Date(x.calculatedAt).toLocaleString()}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
