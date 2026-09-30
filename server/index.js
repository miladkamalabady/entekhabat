import 'dotenv/config'
import express from 'express'
import cors from 'cors'
import db from './db.js'
import { seed, fetchAll } from './seed.js'
import { hashPwd } from './routes/auth.js'
import { requireAuth, requireAdmin } from './middleware/auth.js'
import pubRoutes      from './routes/publications.js'
import journalRoutes  from './routes/journal.js'
import eventsRoutes   from './routes/events.js'
import sectionsRoutes from './routes/sections.js'
import topicsRoutes   from './routes/topics.js'
import teamRoutes     from './routes/team.js'
import aboutRoutes    from './routes/about.js'
import menuRoutes     from './routes/menu.js'
import brandingRoutes from './routes/branding.js'
import authRoutes     from './routes/auth.js'

const app = express()
app.use(cors({
  origin: process.env.ALLOWED_ORIGIN || '*',
  credentials: true
}))
app.use(express.json({ limit: '10mb' }))

// فقط درخواست‌های write (POST/PUT/DELETE) به احراز هویت نیاز دارند
function authForWrites(req, res, next) {
  if (req.method === 'GET') return next()
  return requireAuth(req, res, next)
}

app.use('/api/auth',         authRoutes)
app.use('/api/publications', authForWrites, pubRoutes)
app.use('/api/journal',      authForWrites, journalRoutes)
app.use('/api/events',       authForWrites, eventsRoutes)
app.use('/api/sections',     authForWrites, sectionsRoutes)
app.use('/api/topics',       authForWrites, topicsRoutes)
app.use('/api/team',         authForWrites, teamRoutes)
app.use('/api/about',        authForWrites, aboutRoutes)
app.use('/api/menu',         authForWrites, menuRoutes)
app.use('/api/branding',     authForWrites, brandingRoutes)

app.get('/api/health', (_, res) => res.json({ ok: true }))

app.post('/api/reset', requireAuth, requireAdmin, async (req, res) => {
  try {
    await seed(true)
    res.json(await fetchAll())
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

const PORT = process.env.PORT || 3000

async function start() {
  await db.query(`CREATE TABLE IF NOT EXISTS publications (
    id BIGINT PRIMARY KEY, json_data LONGTEXT NOT NULL
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4`)
  await db.query(`CREATE TABLE IF NOT EXISTS journal_issues (
    id BIGINT PRIMARY KEY, json_data LONGTEXT NOT NULL
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4`)
  await db.query(`CREATE TABLE IF NOT EXISTS events (
    id BIGINT PRIMARY KEY, json_data LONGTEXT NOT NULL
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4`)
  await db.query(`CREATE TABLE IF NOT EXISTS settings (
    k VARCHAR(100) PRIMARY KEY, v LONGTEXT NOT NULL
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4`)

  await db.query(`CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('admin','editor') DEFAULT 'admin',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4`)
  await db.query(`CREATE TABLE IF NOT EXISTS sessions (
    token VARCHAR(64) PRIMARY KEY,
    user_id INT NOT NULL,
    expires_at DATETIME NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4`)

  await db.query(`CREATE TABLE IF NOT EXISTS team_members (
    id BIGINT PRIMARY KEY, json_data LONGTEXT NOT NULL
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4`)

  // seed team if empty
  const [[{ tc }]] = await db.query('SELECT COUNT(*) as tc FROM team_members')
  if (tc === 0) {
    const { team: seedTeam } = await import('./seed.js').then(m => ({ team: null }))
      .catch(() => ({ team: null }))
    // use direct import
    const { team: teamData } = await import('../src/data/team.js')
    for (const member of teamData) {
      const { id, ...data } = member
      await db.query('INSERT INTO team_members (id, json_data) VALUES (?, ?)', [id, JSON.stringify(data)])
    }
    console.log('✅ Team seeded')
  }

  const [[{ c }]] = await db.query('SELECT COUNT(*) as c FROM users')
  if (c === 0) {
    await db.query(
      'INSERT INTO users (username, password_hash, role) VALUES (?, ?, ?)',
      ['admin', hashPwd('1234'), 'admin']
    )
    console.log('✅ Default admin created  (user: admin / pass: 1234)')
  }

  await seed(false)

  app.listen(PORT, () => {
    console.log(`API ready → http://localhost:${PORT}`)
  })
}

start().catch(err => {
  console.error('Server start failed:', err.message)
  process.exit(1)
})
