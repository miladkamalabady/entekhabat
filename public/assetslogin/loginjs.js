function GetCaptcha() {
      $.ajax({
        type: "GET",
        url: " https://myssl.medu.ir/api/Captcha/GetCaptcha/",
        cache: false,

        success: function (html) {
          let src = html?.data?.hashValue.substr(html?.data?.hashValue?.length - 15, 10);
          var va = html?.data?.value;
          va = va.substr(0, 7).concat(va.substr(8));
          va = va.substr(0, 13).concat(va.substr(14));
          va = va.replace(src, "");
          html.data.value = va;
          src = 'data:image/png;base64,' + html.data.value;
          $("#captchasrc").attr('src', src);
          $("#hiddenvalue").html(html.data.hashValue.toString());
        }
      })
    }

    $(document).ready(function () {
      GetCaptcha()
    })

    $('#submit_notice').submit(function (e) {
      e.preventDefault();
      // $("#form_second_step .alert-danger span").html("در حال بررسی  ...");
      // $("#form_second_step .alert-danger").show();
      // $(".primary-main-color-bg").prop("disabled", true);

      const body = {
        NationalID: $("#NationalID").val(),
        password: $("#password").val(),
      }

      var
        persianNumbers = [/۰/g, /۱/g, /۲/g, /۳/g, /۴/g, /۵/g, /۶/g, /۷/g, /۸/g, /۹/g],
        arabicNumbers = [/٠/g, /١/g, /٢/g, /٣/g, /٤/g, /٥/g, /٦/g, /٧/g, /٨/g, /٩/g],
        fixNumbers = function (str) {
          if (typeof str === 'string')
            for (var i = 0; i < 10; i++)
              str = str.replace(persianNumbers[i], i).replace(arabicNumbers[i], i);
          return str;
        };
      body.NationalID = fixNumbers(body.NationalID)
      body.password = fixNumbers(body.password)

      ///////


      $.ajax({
        type: 'POST',
        headers: {
          'captchaResponse': fixNumbers($("#captchaResponse").val()),
          'CaptchaHash': $("#hiddenvalue").html(),
          'Content-Type': "application/json; charset=UTF-8",
        },
        url: 'https://myssl.medu.ir/api/Login/Login',
        data: JSON.stringify(body),
        dataType: 'json',
        async: true,
        success: function (data) {
          if (data.resultCode == 200) {
            
            $('#successInfo').html("در حال ورود...")
            $('#myModal').modal("show")
            let response=data
            let img = "/assets/img/avatar.png"
          if (response.data.result.photo != null)
            img = response.data.result.photo
          // let fullname = response.data.result.firstName + " " + response.data.result.lastName
          let role = 4
          if (response.data.result.type == 2 || response.data.result.type == 11) {
            role = 4
          }
          else if (response.data.result.type == 5 || response.data.result.type == 9 || response.data.result.type == 10) {
            img = "/assets/img/avatarst.png"
            if (response.data.result.avatar != null && response.data.result.avatar != 'null')
              img = "/assets/img/avatars/" + response.data.result.avatar.toLowerCase() + ".png"

            if (response?.data?.result?.student?.stageId && response?.data?.result?.student?.stageId > 1)
              role = 5
            else
              role = 6
          }
          else if (response.data.result.type == 3 || response.data.result.type == 8)
            role = 2
          const item = {
            img: img,
            role: role,
            result: response.data.result
          }
          localStorage.setItem('user', JSON.stringify(item))

          setTimeout(() => {
            if (role == 2)
              window.location.href = 'app/dashboards/teacher';
            else
              if (role == 4)
                window.location.href = `app/dashboards/people`;
              else if (role == 6)
                window.location.href = `app/dashboards/NoAmoz`;
              else if (role == 5) 
                window.location.href = `app/dashboards/student`;
              
          }, 1000);
          
          } else {
            $('#successInfo').html(data.data)
            $('#myModal').modal("show")
            GetCaptcha()
          }
        },
        cache: false,
        contentType: false,
        processData: false
      });
    });
