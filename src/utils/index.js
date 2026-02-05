import { defaultDirection, defaultColor, themeSelectedColorStorageKey, themeRadiusStorageKey, localeOptions, defaultLocale } from '../constants/config'

export const mapOrder = (array, order, key) => {
  array.sort(function (a, b) {
    var A = a[key]
    var B = b[key]
    if (order.indexOf(A + '') > order.indexOf(B + '')) {
      return 1
    } else {
      return -1
    }
  })
  return array
}
export const fixNumbers = (str) => {
  var persianNumbers = [/۰/g, /۱/g, /۲/g, /۳/g, /۴/g, /۵/g, /۶/g, /۷/g, /۸/g, /۹/g];
  var arabicNumbers = [/٠/g, /١/g, /٢/g, /٣/g, /٤/g, /٥/g, /٦/g, /٧/g, /٨/g, /٩/g];
  if (typeof str === 'string')
    for (var i = 0; i < 10; i++)
      str = str.replace(persianNumbers[i], i).replace(arabicNumbers[i], i);
  return str;
};
export const convertDate = (val) => {
  // let tav = val;
  //   const p2e = (s) => s.replace(/[۰-۹]/g, (d) => "۰۱۲۳۴۵۶۷۸۹".indexOf(d));
  // if (val) {
  //   tav = val.substr(0, 10);
  //   tav =
  //     new Date(tav).toLocaleDateString("fa-IR", {
  //       year: "numeric",
  //       month: "2-digit",
  //       day: "2-digit",
  //       formatMatcher: "basic",
  //     })
  //     tav=p2e(tav)
  // }
  // return (tav);
  // return  this.$moment(val, "YYYY/MM/DD").format(
  //             "jYYYY/jMM/jDD"
  //           ) 
};
export const getclasscolor = (key) => {
  if (key < 6)
    return key % 6 == 0 ? `img7` : key % 6 == 1 ? `img2` : key % 6 == 2 ? `img3` : key % 6 == 3 ? `img4` : key % 6 == 4 ? `img5` : `img6`
  else
    return key % 6 == 0 ? `img6` : key % 6 == 1 ? `img7` : key % 6 == 2 ? `img2` : key % 6 == 3 ? `img3` : key % 6 == 4 ? `img4` : `img5`
}
export const getDateWithFormat = () => {
  const today = new Date()
  let dd = today.getDate()
  let mm = today.getMonth() + 1 // January is 0!

  var yyyy = today.getFullYear()
  if (dd < 10) {
    dd = '0' + dd
  }
  if (mm < 10) {
    mm = '0' + mm
  }
  return dd + '.' + mm + '.' + yyyy
}

export const getCurrentTime = () => {
  const now = new Date()
  return now.getHours() + ':' + now.getMinutes()
}

export const ThemeColors = () => {
  let rootStyle = getComputedStyle(document.body)
  return {
    themeColor1: rootStyle.getPropertyValue('--theme-color-1').trim(),
    themeColor2: rootStyle.getPropertyValue('--theme-color-2').trim(),
    themeColor3: rootStyle.getPropertyValue('--theme-color-3').trim(),
    themeColor4: rootStyle.getPropertyValue('--theme-color-4').trim(),
    themeColor5: rootStyle.getPropertyValue('--theme-color-5').trim(),
    themeColor6: rootStyle.getPropertyValue('--theme-color-6').trim(),
    themeColor1_10: rootStyle.getPropertyValue('--theme-color-1-10').trim(),
    themeColor2_10: rootStyle.getPropertyValue('--theme-color-2-10').trim(),
    themeColor3_10: rootStyle.getPropertyValue('--theme-color-3-10').trim(),
    themeColor4_10: rootStyle.getPropertyValue('--theme-color-3-10').trim(),
    themeColor5_10: rootStyle.getPropertyValue('--theme-color-3-10').trim(),
    themeColor6_10: rootStyle.getPropertyValue('--theme-color-3-10').trim(),
    primaryColor: rootStyle.getPropertyValue('--primary-color').trim(),
    foregroundColor: rootStyle.getPropertyValue('--foreground-color').trim(),
    separatorColor: rootStyle.getPropertyValue('--separator-color').trim()
  }
}

export const getDirection = () => {
  let direction = defaultDirection
  if (localStorage.getItem('direction')) {
    const localValue = localStorage.getItem('direction')
    if (localValue === 'rtl' || localValue === 'ltr') {
      direction = localValue
    }
  }
  return {
    direction,
    isRtl: direction === 'rtl'
  }
}

export const setDirection = localValue => {
  let direction = 'ltr'
  if (localValue === 'rtl' || localValue === 'ltr') {
    direction = localValue
  }
  localStorage.setItem('direction', direction)
}


export const getThemeColor = () => {
  let color = defaultColor;
  try {
    if (localStorage.getItem(themeSelectedColorStorageKey)) {
      color = localStorage.getItem(themeSelectedColorStorageKey) || defaultColor;
    }
  } catch (error) {
    console.log(">>>> src/utils/index.js : getThemeColor -> error", error)
    color = defaultColor;
  }
  return color;
}

export const setThemeColor = (color) => {
  try {
    localStorage.setItem(themeSelectedColorStorageKey, color);
  } catch (error) {
    console.log(">>>> src/utils/index.js : setThemeColor -> error", error)
  }
}

export const getThemeRadius = () => {
  let radius = "rounded";
  try {
    if (localStorage.getItem(themeRadiusStorageKey)) {
      radius = localStorage.getItem(themeRadiusStorageKey) || "rounded";
    }
  } catch (error) {
    console.log(">>>> src/utils/index.js : getThemeRadius -> error", error)
    radius = "rounded";
  }
  return radius;
}

export const setThemeRadius = (radius) => {
  try {
    localStorage.setItem(themeRadiusStorageKey, radius);
  } catch (error) {
    console.log(">>>> src/utils/index.js : setThemeRadius -> error", error)
  }
}

export const getCurrentLanguage = () => {
  let locale = defaultLocale;
  try {
    if (localStorage.getItem('currentLanguage') && localeOptions.filter(x => x.id === localStorage.getItem('currentLanguage')).length > 0) { locale = localStorage.getItem('currentLanguage'); }
  } catch (error) {
    console.log(">>>> src/utils/index.js : getCurrentLanguage -> error", error)
    locale = defaultLocale;
  }
  return locale;
}


export const isMobile = () => {
  if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
    return true
  } else {
    return false
  }
}

export const getCurrentUser = () => {

  let user = null;
  try {

    user = localStorage.getItem('user') != null ? JSON.parse(localStorage.getItem('user')) : null;

    if (user) {

      if (!user.token)
        user.token = null
    }
    
  } catch (error) {
    console.log(">>>> src/utils/index.js : getCurrentUser -> error", error)
    user = null;
  }
  return user;
}


export const findEmploymentStatus = (status) => {
  let ret = status;
  switch (status) {
    case 0: ret = 'نامشخص'
      break;
    case 1: ret = 'جذب و استخدام'
      break;
    case 2: ret = 'شاغل'
      break;
    case 3: ret = 'بازنشسته'
      break;
    case 4: ret = 'فوتی شاغل'
      break;
    case 5: ret = 'فوتی بازنشسته'
      break;
    case 6: ret = 'انفصال'
      break;
    case 7: ret = 'استخدام شده'
      break;
    case 15: ret = 'تعلیق'
      break;
    case 21: ret = 'غایب'
      break;
    case 24: ret = 'اخراج'
      break;
    case 26: ret = 'استعفا'
      break;
    case 27: ret = 'بازخرید'
      break;
    case 28: ret = 'انتقال درون سازمانی'
      break;
    case 30: ret = 'انتقال موقت به سایر سازمان ها'
      break;
    case 32: ret = 'انتقال یا مامور به سایر سازمان ها'
      break;
    case 33: ret = 'انفصال موقت'
      break;
    case 34: ret = 'انتقال دائم به سایر سازمان ها'
      break;
    case 35: ret = 'نامشخص'
      break;
    case 36: ret = 'غیرقابل قبول'
      break;
    case 37: ret = 'مازاد'
      break;
    case 38: ret = 'انصراف'
      break;

  }
  return ret;
}


