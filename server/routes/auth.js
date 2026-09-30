import { Router } from 'express'
import crypto from 'crypto'
import db from '../db.js'
import { requireAuth, requireAdmin } from '../middleware/auth.js'

const router = Router()

function hashPwd(pwd) {
  return crypto.createHash('sha256').update(pwd + 'tt-salt-2024').digest('hex')
}
function makeToken() {
  return crypto.randomBytes(32).toString('hex')
}

// POST /api/auth/login
router.post('/login', async (req, res) => {
  try {
    const { username, password } = req.body
    if (!username || !password) return res.status(400).json({ error: 'اطلاعات ناقص است.' })

    const [rows] = await db.query(
      'SELECT id, username, role FROM users WHERE username = ? AND password_hash = ?',
      [username.trim(), hashPwd(password)]
    )
    if (!rows.length) return res.status(401).json({ error: 'نام کاربری یا رمز اشتباه است.' })

    const user = rows[0]
    const token = makeToken()
    const expires = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000) // 7 days

    await db.query(
      'INSERT INTO sessions (token, user_id, expires_at) VALUES (?, ?, ?)',
      [token, user.id, expires]
    )
    res.json({ token, user: { id: user.id, username: user.username, role: user.role } })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

// POST /api/auth/logout
router.post('/logout', requireAuth, async (req, res) => {
  try {
    const token = req.headers.authorization?.replace('Bearer ', '')
    if (token) await db.query('DELETE FROM sessions WHERE token = ?', [token])
    res.json({ ok: true })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

// GET /api/auth/me
router.get('/me', requireAuth, (req, res) => {
  res.json({ user: req.user })
})

// GET /api/auth/users  (admin only)
router.get('/users', requireAuth, requireAdmin, async (req, res) => {
  try {
    const [rows] = await db.query('SELECT id, username, role FROM users ORDER BY id')
    res.json(rows)
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

// POST /api/auth/users  (admin only)
router.post('/users', requireAuth, requireAdmin, async (req, res) => {
  try {
    const { username, password, role } = req.body
    if (!username || !password) return res.status(400).json({ error: 'نام کاربری و رمز الزامی است.' })

    const [existing] = await db.query('SELECT id FROM users WHERE username = ?', [username.trim()])
    if (existing.length) return res.status(400).json({ error: 'این نام کاربری قبلاً ثبت شده.' })

    const [result] = await db.query(
      'INSERT INTO users (username, password_hash, role) VALUES (?, ?, ?)',
      [username.trim(), hashPwd(password), role === 'editor' ? 'editor' : 'admin']
    )
    res.status(201).json({ id: result.insertId, username: username.trim(), role: role || 'admin' })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

// DELETE /api/auth/users/:id  (admin only, can't delete self)
router.delete('/users/:id', requireAuth, requireAdmin, async (req, res) => {
  try {
    const id = Number(req.params.id)
    if (id === req.user.id) return res.status(400).json({ error: 'نمی‌توانید خودتان را حذف کنید.' })

    const [[{ c }]] = await db.query('SELECT COUNT(*) as c FROM users WHERE role = "admin"')
    if (c <= 1) {
      const [[target]] = await db.query('SELECT role FROM users WHERE id = ?', [id])
      if (target && target.role === 'admin') {
        return res.status(400).json({ error: 'باید حداقل یک مدیر وجود داشته باشد.' })
      }
    }

    await db.query('DELETE FROM sessions WHERE user_id = ?', [id])
    await db.query('DELETE FROM users WHERE id = ?', [id])
    res.json({ ok: true })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

// PUT /api/auth/change-password
router.put('/change-password', requireAuth, async (req, res) => {
  try {
    const { currentPassword, newPassword } = req.body
    if (!currentPassword || !newPassword) return res.status(400).json({ error: 'اطلاعات ناقص است.' })
    if (newPassword.length < 4) return res.status(400).json({ error: 'رمز جدید باید حداقل ۴ کاراکتر باشد.' })

    const [rows] = await db.query(
      'SELECT id FROM users WHERE id = ? AND password_hash = ?',
      [req.user.id, hashPwd(currentPassword)]
    )
    if (!rows.length) return res.status(401).json({ error: 'رمز فعلی اشتباه است.' })

    await db.query('UPDATE users SET password_hash = ? WHERE id = ?', [hashPwd(newPassword), req.user.id])
    res.json({ ok: true })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

export { hashPwd }
export default router
