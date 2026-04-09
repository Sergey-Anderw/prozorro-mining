import { jsx as _jsx, jsxs as _jsxs } from "react/jsx-runtime";
import { useEffect, useState } from 'react';
import './App.css';
const currency = new Intl.NumberFormat('uk-UA', {
    style: 'currency',
    currency: 'UAH',
    maximumFractionDigits: 0,
});
const dateTime = new Intl.DateTimeFormat('uk-UA', {
    dateStyle: 'medium',
    timeStyle: 'short',
});
function App() {
    const [dashboard, setDashboard] = useState(null);
    const [status, setStatus] = useState(null);
    const [loadingDashboard, setLoadingDashboard] = useState(true);
    const [loadingStatus, setLoadingStatus] = useState(true);
    const [startingImport, setStartingImport] = useState(false);
    const [error, setError] = useState(null);
    useEffect(() => {
        void loadDashboard();
        void loadStatus();
    }, []);
    useEffect(() => {
        if (!status?.importRunId || status.status !== 'Running') {
            return;
        }
        const intervalId = window.setInterval(() => {
            void loadStatus(status.importRunId);
        }, 5000);
        return () => window.clearInterval(intervalId);
    }, [status?.importRunId, status?.status]);
    async function loadDashboard() {
        setLoadingDashboard(true);
        try {
            const response = await fetch('/api/v1/analytics/dashboard');
            if (!response.ok) {
                throw new Error(`Dashboard request failed with ${response.status}`);
            }
            const data = (await response.json());
            setDashboard(data);
            setError(null);
        }
        catch (requestError) {
            setError(getErrorMessage(requestError, 'Не вдалося завантажити аналітику.'));
        }
        finally {
            setLoadingDashboard(false);
        }
    }
    async function loadStatus(importRunId) {
        setLoadingStatus(true);
        try {
            const url = importRunId
                ? `/api/v1/import/status?importRunId=${importRunId}`
                : '/api/v1/import/status';
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`Import status request failed with ${response.status}`);
            }
            const data = (await response.json());
            setStatus(data);
            setError(null);
        }
        catch (requestError) {
            setError(getErrorMessage(requestError, 'Не вдалося завантажити статус імпорту.'));
        }
        finally {
            setLoadingStatus(false);
        }
    }
    async function startImport() {
        setStartingImport(true);
        try {
            const response = await fetch('/api/v1/import/run', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: '{}',
            });
            if (!response.ok) {
                throw new Error(`Import start failed with ${response.status}`);
            }
            const data = (await response.json());
            await Promise.all([loadStatus(data.importRunId), loadDashboard()]);
            setError(null);
        }
        catch (requestError) {
            setError(getErrorMessage(requestError, 'Не вдалося запустити імпорт.'));
        }
        finally {
            setStartingImport(false);
        }
    }
    return (_jsxs("main", { className: "page", children: [_jsxs("section", { className: "hero", children: [_jsxs("div", { children: [_jsx("p", { className: "eyebrow", children: "ProzorroMining" }), _jsx("h1", { children: "\u0410\u043D\u0430\u043B\u0456\u0442\u0438\u043A\u0430 \u0437\u0430\u043A\u0443\u043F\u0456\u0432\u0435\u043B\u044C \u0435\u043B\u0435\u043A\u0442\u0440\u043E\u0435\u043D\u0435\u0440\u0433\u0456\u0457" }), _jsx("p", { className: "subtitle", children: "\u041E\u0433\u043B\u044F\u0434 \u0435\u043A\u043E\u043D\u043E\u043C\u0456\u0457 \u0431\u044E\u0434\u0436\u0435\u0442\u0443 \u0442\u0430 \u043A\u043B\u044E\u0447\u043E\u0432\u0438\u0445 \u0443\u0447\u0430\u0441\u043D\u0438\u043A\u0456\u0432 \u043D\u0430 \u043E\u0441\u043D\u043E\u0432\u0456 \u0437\u0431\u0435\u0440\u0435\u0436\u0435\u043D\u0438\u0445 \u0434\u0430\u043D\u0438\u0445 Prozorro." })] }), _jsxs("div", { className: "hero-actions", children: [_jsx("button", { className: "primary-button", onClick: startImport, disabled: startingImport, children: startingImport ? 'Запуск імпорту...' : 'Оновити дані' }), _jsx("button", { className: "secondary-button", onClick: () => void Promise.all([loadDashboard(), loadStatus(status?.importRunId)]), children: "\u041E\u043D\u043E\u0432\u0438\u0442\u0438 dashboard" })] })] }), error ? _jsx("div", { className: "alert", children: error }) : null, _jsxs("section", { className: "summary-grid", children: [_jsxs("article", { className: "summary-card summary-card-accent", children: [_jsx("span", { className: "summary-label", children: "\u0417\u0430\u0433\u0430\u043B\u044C\u043D\u0430 \u0435\u043A\u043E\u043D\u043E\u043C\u0456\u044F" }), _jsx("strong", { className: "summary-value", children: loadingDashboard ? 'Завантаження...' : currency.format(dashboard?.totalSavings ?? 0) }), _jsx("span", { className: "summary-note", children: "\u0420\u0456\u0437\u043D\u0438\u0446\u044F \u043C\u0456\u0436 \u043E\u0447\u0456\u043A\u0443\u0432\u0430\u043D\u043E\u044E \u0432\u0430\u0440\u0442\u0456\u0441\u0442\u044E \u0442\u0430 \u0441\u0443\u043C\u043E\u044E \u043A\u043E\u043D\u0442\u0440\u0430\u043A\u0442\u0456\u0432." })] }), _jsxs("article", { className: "summary-card", children: [_jsx("span", { className: "summary-label", children: "\u041E\u0441\u0442\u0430\u043D\u043D\u0456\u0439 \u0456\u043C\u043F\u043E\u0440\u0442" }), _jsx("strong", { className: "summary-value summary-value-small", children: loadingStatus ? 'Завантаження...' : status?.status ?? 'Pending' }), _jsx("span", { className: "summary-note", children: status?.lastRunAt ? `Старт: ${dateTime.format(new Date(status.lastRunAt))}` : 'Імпорт ще не запускався.' })] }), _jsxs("article", { className: "summary-card", children: [_jsx("span", { className: "summary-label", children: "\u041E\u0431\u0440\u043E\u0431\u043B\u0435\u043D\u043E \u0442\u0435\u043D\u0434\u0435\u0440\u0456\u0432" }), _jsx("strong", { className: "summary-value summary-value-small", children: loadingStatus ? 'Завантаження...' : status?.processedCount ?? 0 }), _jsx("span", { className: "summary-note", children: status?.importRunId ? `Run #${status.importRunId}` : 'Немає активного run.' })] })] }), _jsxs("section", { className: "status-panel", children: [_jsxs("div", { className: "status-row", children: [_jsx("span", { children: "Inserted" }), _jsx("strong", { children: status?.insertedCount ?? 0 })] }), _jsxs("div", { className: "status-row", children: [_jsx("span", { children: "Updated" }), _jsx("strong", { children: status?.updatedCount ?? 0 })] }), _jsxs("div", { className: "status-row", children: [_jsx("span", { children: "Failed" }), _jsx("strong", { children: status?.failedCount ?? 0 })] }), _jsxs("div", { className: "status-row", children: [_jsx("span", { children: "Finished" }), _jsx("strong", { children: status?.finishedAt ? dateTime.format(new Date(status.finishedAt)) : 'In progress' })] })] }), status?.errorMessage ? _jsx("div", { className: "alert alert-error", children: status.errorMessage }) : null, _jsxs("section", { className: "content-grid", children: [_jsxs("article", { className: "panel", children: [_jsxs("div", { className: "panel-header", children: [_jsx("h2", { children: "\u0422\u043E\u043F-5 \u0437\u0430\u043A\u0443\u043F\u0456\u0432\u0435\u043B\u044C\u043D\u0438\u043A\u0456\u0432" }), _jsx("span", { children: "\u0417\u0430 \u0441\u0443\u043C\u043E\u044E \u043F\u0456\u0434\u043F\u0438\u0441\u0430\u043D\u0438\u0445 \u043A\u043E\u043D\u0442\u0440\u0430\u043A\u0442\u0456\u0432" })] }), _jsx(SimpleTable, { loading: loadingDashboard, emptyMessage: "\u0429\u0435 \u043D\u0435\u043C\u0430\u0454 \u0434\u0430\u043D\u0438\u0445 \u043F\u043E \u0437\u0430\u043A\u0443\u043F\u0456\u0432\u0435\u043B\u044C\u043D\u0438\u043A\u0430\u0445.", columns: ['Покупець', 'Сума контрактів', 'Економія'], rows: (dashboard?.topProcurers ?? []).map((row) => [
                                    row.name,
                                    currency.format(row.totalContractValue),
                                    currency.format(row.totalSavings),
                                ]) })] }), _jsxs("article", { className: "panel", children: [_jsxs("div", { className: "panel-header", children: [_jsx("h2", { children: "\u0422\u043E\u043F-5 \u043F\u043E\u0441\u0442\u0430\u0447\u0430\u043B\u044C\u043D\u0438\u043A\u0456\u0432" }), _jsx("span", { children: "\u0417\u0430 \u0441\u0443\u043C\u043E\u044E \u043F\u0456\u0434\u043F\u0438\u0441\u0430\u043D\u0438\u0445 \u043A\u043E\u043D\u0442\u0440\u0430\u043A\u0442\u0456\u0432" })] }), _jsx(SimpleTable, { loading: loadingDashboard, emptyMessage: "\u0429\u0435 \u043D\u0435\u043C\u0430\u0454 \u0434\u0430\u043D\u0438\u0445 \u043F\u043E \u043F\u043E\u0441\u0442\u0430\u0447\u0430\u043B\u044C\u043D\u0438\u043A\u0430\u0445.", columns: ['Постачальник', 'Контракти', 'Сума'], rows: (dashboard?.topSuppliers ?? []).map((row) => [
                                    row.name,
                                    row.contractCount.toString(),
                                    currency.format(row.totalValue),
                                ]) })] })] })] }));
}
function SimpleTable({ loading, emptyMessage, columns, rows }) {
    if (loading) {
        return _jsx("div", { className: "empty-state", children: "\u0417\u0430\u0432\u0430\u043D\u0442\u0430\u0436\u0435\u043D\u043D\u044F..." });
    }
    if (rows.length === 0) {
        return _jsx("div", { className: "empty-state", children: emptyMessage });
    }
    return (_jsx("div", { className: "table-wrap", children: _jsxs("table", { children: [_jsx("thead", { children: _jsx("tr", { children: columns.map((column) => (_jsx("th", { children: column }, column))) }) }), _jsx("tbody", { children: rows.map((row, index) => (_jsx("tr", { children: row.map((cell) => (_jsx("td", { children: cell }, `${row[0]}-${cell}`))) }, `${row[0]}-${index}`))) })] }) }));
}
function getErrorMessage(error, fallbackMessage) {
    return error instanceof Error ? error.message : fallbackMessage;
}
export default App;
