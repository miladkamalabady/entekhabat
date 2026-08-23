import db from '../db.js'

export async function requireAuth(req, res, next) {
  try {
    const token = req.headers.authorization?.replace('Bearer ', '')
    if (!token) return res.status(401).json({ error: 'احراز هویت لازم است.' })

    const [rows] = await db.query(
      `SELECT u.id, u.username, u.role
       FROM sessions s JOIN users u ON s.user_id = u.id
       WHERE s.token = ? AND s.expires_at > NOW()`,
      [token]
    )
    if (!rows.length) return res.status(401).json({ error: 'نشست منقضی شده. لطفاً دوباره وارد شوید.' })

    req.user = rows[0]
    next()
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
}

export function requireAdmin(req, res, next) {
  if (req.user?.role !== 'admin') {
    return res.status(403).json({ error: 'دسترسی فقط برای مدیر.' })
  }
  next()
}
