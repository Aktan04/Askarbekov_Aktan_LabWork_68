// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#togglePassword').click(function () {
        var passwordField = $('#password');
        var fieldType = passwordField.attr('type');
        if (fieldType === 'password') {
            passwordField.attr('type', 'text');
            $(this).find('i').removeClass('fa-eye-slash').addClass('fa-eye');
        } else {
            passwordField.attr('type', 'password');
            $(this).find('i').removeClass('fa-eye').addClass('fa-eye-slash');
        }
    });

    $('#toggleConfirmPassword').click(function () {
        var confirmPasswordField = $('#confirmPassword');
        var fieldType = confirmPasswordField.attr('type');
        if (fieldType === 'password') {
            confirmPasswordField.attr('type', 'text');
            $(this).find('i').removeClass('fa-eye-slash').addClass('fa-eye');
        } else {
            confirmPasswordField.attr('type', 'password');
            $(this).find('i').removeClass('fa-eye').addClass('fa-eye-slash');
        }
    });
});

$(document).ready(function () {
    $('.edit-profile-btn').click(function () {
        
        var userId = $(this).data('user-id');
        var userName = $(this).data('user-name');
        var nickName = $(this).data('user-nickname');
        var email = $(this).data('user-email');
        var avatar = $(this).data('user-avatar');

        $('#editUserId').val(userId);
        $('#editUserName').val(userName);
        $('#editNickName').val(nickName);
        $('#editEmail').val(email);

        if (avatar) {
            $('#editProfileModal .profile-avatar').attr('src', avatar);
        }

        $('#editProfileModal').modal('show');
    });

    $('#editProfileForm').submit(function (event) {
        event.preventDefault();
        var isValid = true;
        var userName = $('#editUserName').val().trim();
        var nickName = $('#editNickName').val().trim();
        var email = $('#editEmail').val().trim();
        $('#editUserNameValidation').text('');
        $('#editNickNameValidation').text('');
        $('#editEmailValidation').text('');
        $('#editImageFileValidation').text('');
        $('#editFormErrors').text('');
        if (!userName) {
            $('#editUserNameValidation').text('Имя обязательно к заполнению');
            isValid = false;
        }
        if (!nickName) {
            $('#editNickNameValidation').text('Никнейм обязателен к заполнению');
            isValid = false;
        }
        if (!email) {
            $('#editEmailValidation').text('Email обязателен к заполнению');
            isValid = false;
        }

        if (isValid) {
            var formData = new FormData(this);
            $.ajax({
                url: '/Account/EditProfile',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.success) {
                        $('#editProfileModal').modal('hide');
                        var updatedUser = response.user;
                        $('#profileUserName').text(updatedUser.userName);
                        $('#profileNickName').text(updatedUser.nickName);
                        $('#profileEmail').text(updatedUser.email);
                        $('#userUserName').text(updatedUser.userName);
                        $('#userNickName').text(updatedUser.nickName);
                        $('#userEmail').text(updatedUser.email);
                        if (updatedUser.avatar) {
                            $('.profile-avatar').attr('src', updatedUser.avatar);
                        }
                        $('.edit-profile-btn').data('user-name', updatedUser.userName);
                        $('.edit-profile-btn').data('user-nickname', updatedUser.nickName);
                        $('.edit-profile-btn').data('user-email', updatedUser.email);
                    }
                    else 
                    {
                        $('#editProfileForm .text-danger').text('');

                        response.errors.forEach(function(error) {
                            $('#editFormErrors').append('<div>' + error + '</div>');
                        });
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Ошибка при отправке AJAX-запроса:', error);
                } 
            });
        }
    });
});