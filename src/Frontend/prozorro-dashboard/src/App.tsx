import { useEffect, useState } from 'react'
import './App.css'

type DashboardResponse = {
  totalSavings: number
  topProcurers: ProcurerInfo[]
  topSuppliers: SupplierInfo[]
}

type ProcurerInfo = {
  name: string
  totalContractValue: number
  totalSavings: number
}

type SupplierInfo = {
  name: string
  contractCount: number
  totalValue: number
}

type ImportStatusResponse = {
  importRunId?: number | null
  status: string
  lastRunAt?: string | null
  finishedAt?: string | null
  processedCount: number
  insertedCount: number
  updatedCount: number
  failedCount: number
  errorMessage?: string | null
}

const currency = new Intl.NumberFormat('uk-UA', {
  style: 'currency',
  currency: 'UAH',
  maximumFractionDigits: 0,
})

const dateTime = new Intl.DateTimeFormat('uk-UA', {
  dateStyle: 'medium',
  timeStyle: 'short',
})

function App() {
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null)
  const [status, setStatus] = useState<ImportStatusResponse | null>(null)
  const [loadingDashboard, setLoadingDashboard] = useState(true)
  const [loadingStatus, setLoadingStatus] = useState(true)
  const [startingImport, setStartingImport] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    void loadDashboard()
    void loadStatus()
  }, [])

  useEffect(() => {
    if (!status?.importRunId || status.status !== 'Running') {
      return
    }

    const intervalId = window.setInterval(() => {
      void loadStatus(status.importRunId)
    }, 5000)

    return () => window.clearInterval(intervalId)
  }, [status?.importRunId, status?.status])

  async function loadDashboard() {
    setLoadingDashboard(true)

    try {
      const response = await fetch('/api/v1/analytics/dashboard')
      if (!response.ok) {
        throw new Error(`Dashboard request failed with ${response.status}`)
      }

      const data = (await response.json()) as DashboardResponse
      setDashboard(data)
      setError(null)
    } catch (requestError) {
      setError(getErrorMessage(requestError, 'Не вдалося завантажити аналітику.'))
    } finally {
      setLoadingDashboard(false)
    }
  }

  async function loadStatus(importRunId?: number | null) {
    setLoadingStatus(true)

    try {
      const url = importRunId
        ? `/api/v1/import/status?importRunId=${importRunId}`
        : '/api/v1/import/status'

      const response = await fetch(url)
      if (!response.ok) {
        throw new Error(`Import status request failed with ${response.status}`)
      }

      const data = (await response.json()) as ImportStatusResponse
      setStatus(data)
      setError(null)
    } catch (requestError) {
      setError(getErrorMessage(requestError, 'Не вдалося завантажити статус імпорту.'))
    } finally {
      setLoadingStatus(false)
    }
  }

  async function startImport() {
    setStartingImport(true)

    try {
      const response = await fetch('/api/v1/import/run', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: '{}',
      })

      if (!response.ok) {
        throw new Error(`Import start failed with ${response.status}`)
      }

      const data = (await response.json()) as {
        importRunId: number
        status: string
      }

      await Promise.all([loadStatus(data.importRunId), loadDashboard()])
      setError(null)
    } catch (requestError) {
      setError(getErrorMessage(requestError, 'Не вдалося запустити імпорт.'))
    } finally {
      setStartingImport(false)
    }
  }

  return (
    <main className="page">
      <section className="hero">
        <div>
          <p className="eyebrow">ProzorroMining</p>
          <h1>Аналітика закупівель електроенергії</h1>
          <p className="subtitle">
            Огляд економії бюджету та ключових учасників на основі збережених даних Prozorro.
          </p>
        </div>
        <div className="hero-actions">
          <button className="primary-button" onClick={startImport} disabled={startingImport}>
            {startingImport ? 'Запуск імпорту...' : 'Оновити дані'}
          </button>
          <button className="secondary-button" onClick={() => void Promise.all([loadDashboard(), loadStatus(status?.importRunId)])}>
            Оновити dashboard
          </button>
        </div>
      </section>

      {error ? <div className="alert">{error}</div> : null}

      <section className="summary-grid">
        <article className="summary-card summary-card-accent">
          <span className="summary-label">Загальна економія</span>
          <strong className="summary-value">
            {loadingDashboard ? 'Завантаження...' : currency.format(dashboard?.totalSavings ?? 0)}
          </strong>
          <span className="summary-note">Різниця між очікуваною вартістю та сумою контрактів.</span>
        </article>

        <article className="summary-card">
          <span className="summary-label">Останній імпорт</span>
          <strong className="summary-value summary-value-small">
            {loadingStatus ? 'Завантаження...' : status?.status ?? 'Pending'}
          </strong>
          <span className="summary-note">
            {status?.lastRunAt ? `Старт: ${dateTime.format(new Date(status.lastRunAt))}` : 'Імпорт ще не запускався.'}
          </span>
        </article>

        <article className="summary-card">
          <span className="summary-label">Оброблено тендерів</span>
          <strong className="summary-value summary-value-small">
            {loadingStatus ? 'Завантаження...' : status?.processedCount ?? 0}
          </strong>
          <span className="summary-note">
            {status?.importRunId ? `Run #${status.importRunId}` : 'Немає активного run.'}
          </span>
        </article>
      </section>

      <section className="status-panel">
        <div className="status-row">
          <span>Inserted</span>
          <strong>{status?.insertedCount ?? 0}</strong>
        </div>
        <div className="status-row">
          <span>Updated</span>
          <strong>{status?.updatedCount ?? 0}</strong>
        </div>
        <div className="status-row">
          <span>Failed</span>
          <strong>{status?.failedCount ?? 0}</strong>
        </div>
        <div className="status-row">
          <span>Finished</span>
          <strong>{status?.finishedAt ? dateTime.format(new Date(status.finishedAt)) : 'In progress'}</strong>
        </div>
      </section>

      {status?.errorMessage ? <div className="alert alert-error">{status.errorMessage}</div> : null}

      <section className="content-grid">
        <article className="panel">
          <div className="panel-header">
            <h2>Топ-5 закупівельників</h2>
            <span>За сумою підписаних контрактів</span>
          </div>
          <SimpleTable
            loading={loadingDashboard}
            emptyMessage="Ще немає даних по закупівельниках."
            columns={['Покупець', 'Сума контрактів', 'Економія']}
            rows={(dashboard?.topProcurers ?? []).map((row) => [
              row.name,
              currency.format(row.totalContractValue),
              currency.format(row.totalSavings),
            ])}
          />
        </article>

        <article className="panel">
          <div className="panel-header">
            <h2>Топ-5 постачальників</h2>
            <span>За сумою підписаних контрактів</span>
          </div>
          <SimpleTable
            loading={loadingDashboard}
            emptyMessage="Ще немає даних по постачальниках."
            columns={['Постачальник', 'Контракти', 'Сума']}
            rows={(dashboard?.topSuppliers ?? []).map((row) => [
              row.name,
              row.contractCount.toString(),
              currency.format(row.totalValue),
            ])}
          />
        </article>
      </section>
    </main>
  )
}

type SimpleTableProps = {
  loading: boolean
  emptyMessage: string
  columns: string[]
  rows: string[][]
}

function SimpleTable({ loading, emptyMessage, columns, rows }: SimpleTableProps) {
  if (loading) {
    return <div className="empty-state">Завантаження...</div>
  }

  if (rows.length === 0) {
    return <div className="empty-state">{emptyMessage}</div>
  }

  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={column}>{column}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, index) => (
            <tr key={`${row[0]}-${index}`}>
              {row.map((cell) => (
                <td key={`${row[0]}-${cell}`}>{cell}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function getErrorMessage(error: unknown, fallbackMessage: string) {
  return error instanceof Error ? error.message : fallbackMessage
}

export default App
