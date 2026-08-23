import { Router } from 'express'
import db from '../db.js'

const router = Router()

const defaults = {
  publications: true, journal: true, events: false,
  podcast: false, video: false, gallery: false,
  mainTopics: true, secondaryTopics: true, about: true
}

router.get('/', async (req, res) => {
  try {
    const [rows] = await db.query("SELECT v FROM settings WHERE k = 'sections'")
    res.json(rows.length ? JSON.parse(rows[0].v) : defaults)
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

router.put('/', async (req, res) => {
  try {
    const val = JSON.stringify(req.body)
    await db.query(
      "INSERT INTO settings (k, v) VALUES ('sections', ?) ON DUPLICATE KEY UPDATE v = ?",
      [val, val]
    )
    res.json(req.body)
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

export default router
