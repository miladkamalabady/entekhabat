import { Router } from 'express'
import db from '../db.js'

const router = Router()

const defaults = {
  mission: [
    'اندیشکده نفت و انرژی یک مرکز تحقیقاتی مستقل است که با هدف تولید دانش سیاستی در حوزه انرژی، نفت و گاز فعالیت می‌کند. ما بر این باوریم که تصمیم‌گیری آگاهانه در حوزه انرژی نیازمند تحلیل‌های عمیق، مستقل و مبتنی بر شواهد است.',
    'اندیشکده با گرد هم آوردن پژوهشگران و کارشناسان برجسته از حوزه‌های مختلف اقتصاد، مهندسی، سیاست و محیط زیست، تلاش می‌کند تا دیدگاه‌های چندوجهی و جامعی نسبت به چالش‌ها و فرصت‌های انرژی ایران ارائه دهد.'
  ],
  focuses: [
    { icon: '🛢', title: 'نفت و گاز', desc: 'تحلیل بازار، سیاست‌گذاری تولید و صادرات' },
    { icon: '⚡', title: 'انتقال انرژی', desc: 'انرژی‌های تجدیدپذیر و گذار از سوخت‌های فسیلی' },
    { icon: '🏭', title: 'پالایش و پتروشیمی', desc: 'نوسازی صنعت پالایش و ارتقای زنجیره ارزش' },
    { icon: '🌐', title: 'روابط بین‌الملل انرژی', desc: 'دیپلماسی انرژی و همکاری‌های منطقه‌ای' }
  ]
}

router.get('/', async (req, res) => {
  try {
    const [rows] = await db.query("SELECT v FROM settings WHERE k = 'about_content'")
    res.json(rows.length ? JSON.parse(rows[0].v) : defaults)
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

router.put('/', async (req, res) => {
  try {
    const val = JSON.stringify(req.body)
    await db.query(
      "INSERT INTO settings (k,v) VALUES ('about_content',?) ON DUPLICATE KEY UPDATE v=?",
      [val, val]
    )
    res.json(req.body)
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

export default router
