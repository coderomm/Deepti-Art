$(document).ready(function () {
    // Show Alert Function Common
    function showAlert(alertId, message, iconClass) {
        var alertElement = $('#' + alertId);

        alertElement.removeClass('d-none');
        alertElement.empty();

        if (iconClass && iconClass.trim() !== '') {
            alertElement.append('<i class="' + iconClass + '"></i> ');
        }

        if (message) {
            alertElement.append(message);
        }

        if (alertId !== 'display-alert') {
            alertElement.removeClass('response-msg-lbl');
            alertElement.addClass('response-msg-lbl-h6-style');
        }

        setTimeout(function () {
            alertElement.addClass('d-none');
            alertElement.empty();
            if (alertId !== 'display-alert') {
                alertElement.addClass('response-msg-lbl');
                alertElement.removeClass('response-msg-lbl-h6-style');
            }
        }, 1500);
    }

    function showLoadingSpinner() {
        $("#loading").removeClass('d-none');
        $("#loading").addClass('d-block');
    }

    function hideLoadingSpinner() {
        $("#loading").addClass('d-none');
        $("#loading").removeClass('d-block');
    }

    // Login / Forgot Password / Reset Password Validations
    // Hiding verifyOTP row, verifyOTPAlertMsg, verifyOTPSubmit btn
    $("#otprow").hide();
    $("#verifyOTPAlertMsg").hide();
    $("#verifyOTPSubmit").hide();

    $("#sendOTP").click(function () {
        var Email = $("#Email").val();
        $.ajax({
            type: "POST",
            url: "/user/account/forgot-password",
            data: { Email: Email },
            dataType: 'json',
            beforeSend: showLoadingSpinner,
            success: function (response) {
                if (response.success) {
                    $("#Email").prop('readonly', true);
                    $("#LoginEmailAlertMsg").hide();
                    $("#sendOTP").hide();

                    $("#otprow").show();
                    $("#verifyOTPAlertMsg").show();
                    $("#verifyOTPSubmit").show();

                    showAlert('display-alert', 'OTP sent successfully!', 'fa-solid fa-check-circle');
                    var countdown = 120;
                    displayCountdown(countdown);

                    setTimeout(function () {
                        $('#countdown').hide(); // Hide the countdown
                    }, countdown * 1000); // Convert to milliseconds

                } else {
                    showAlert('LoginEmailAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('LoginEmailAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
            },
            complete: hideLoadingSpinner
        });
    });

    function displayCountdown(seconds) {
        $('#countdown').show();
        var countdownInterval = setInterval(function () {
            $('#countdown').text('Resend OTP in ' + seconds + ' seconds');
            seconds--;
            if (seconds < 0) {
                clearInterval(countdownInterval);
                $('#countdown').hide();
                $("#Email").prop('readonly', false);
                $("#otprow").hide();
                $("#verifyOTPAlertMsg").hide();
                $("#verifyOTPSubmit").hide();

                $("#LoginEmailAlertMsg").show();
                $("#sendOTP").show();
            }
        }, 1000);
    }
    function resetCountdown() {
        $('#countdown').hide();
        $("#sendOTP").prop('disabled', false);
    }
    $("#verifyOTPSubmit").click(function () {
        var otp = $("#verifyOTP").val();
        $.ajax({
            type: "POST",
            url: "/user/account/forgot-password/verify-otp",
            data: { verifyOTP: otp },
            dataType: 'json',
            beforeSend: showLoadingSpinner,
            success: function (response) {
                if (response.success) {
                    showAlert('display-alert', 'OTP Verified Successfully!', 'fa-solid fa-check-circle');
                    window.location.href = "/user/account/forgot-password/reset-password";

                    var countdown = 120;
                    displayCountdown(countdown);
                } else {
                    // If there's an error, reset the countdown and allow resending
                    resetCountdown();
                    showAlert('verifyOTPAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function (xhr, status, error) {
                console.log("AJAX Error:", status, error);
                showAlert('verifyOTPAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
                // If there's an error, reset the countdown and allow resending
                resetCountdown();
            },
            complete: hideLoadingSpinner
        });
    });

    // Reset Password
    $("#ResetPasswordPasswordSubmit").click(function (e) {
        var password = $("#Password").val();

        e.preventDefault();

        var isValid = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@@#$%^&+=]).{6,}$/.test(password);
        if (!isValid) {
            $("#resetPasswordAlertMsgClientSide").text('Please enter a valid password that meets the criteria.').show();
            setTimeout(function () {
                $("#resetPasswordAlertMsgClientSide").text('').hide();
            }, 5000);
            return;
        }

        $.ajax({
            type: "POST",
            url: "/user/account/forgot-password/reset-password",
            data: { Password: password },
            dataType: 'json',
            beforeSend: showLoadingSpinner,
            success: function (response) {
                if (response.success) {
                    showAlert('display-alert', 'Password changed successfully!', 'fa-solid fa-check-circle');
                    window.location.href = "/user/account/login";
                } else {
                    showAlert('resetPasswordAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('resetPasswordAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
            },
            complete: hideLoadingSpinner
        });
    });

    // SignUp
    $("#signupForm").submit(function (e) {
        e.preventDefault();
        var Email = $("#Email").val();
        var Mobile = $("#Mobile").val();

        if (Email !== "" && Mobile !== "") {
            if (!isValidEmail(Email)) {
                showAlert('SignupLbl', 'Please enter a valid email address', 'fa-solid fa-circle-exclamation');
                return;
            }

            if (!isValidIndianMobileNumber(Mobile)) {
                showAlert('SignupLbl', 'Please enter a valid mobile number', 'fa-solid fa-circle-exclamation');
                return;
            }
        }

        $.ajax({
            type: "POST",
            url: "/user/account/signup",
            data: { Email: Email, Mobile: Mobile },
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    showAlert('SignupLbl', 'OTP sent successfully', 'fa-solid fa-check-circle');
                    window.location.href = "/user/account/signup/verify-otp";
                }
                else {
                    showAlert('SignupLbl', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('SignupLbl', 'An error occurred during the request', 'fa-solid fa-circle-exclamation');
            }
        });

    });


    // Verify SignUp OTP
    $("#verifySignupOTPSubmit").click(function () {
        var otp = $("#verifySignupOTP").val();
        $.ajax({
            type: "POST",
            url: "/user/account/signup/verify-otp",
            data: { verifyOTP: otp },
            dataType: 'json',
            beforeSend: showLoadingSpinner,
            success: function (response) {
                if (response.success) {
                    showAlert('verifySignupOTPAlertMsg', 'OTP verified successfully!', 'fa-solid fa-check-circle');
                    window.location.href = "/user/account/signup/password";
                } else {
                    showAlert('verifySignupOTPAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('verifySignupOTPAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
            },
            complete: hideLoadingSpinner
        });
    });

    // Set Password
    $("#PasswordForm").submit(function (e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var formData = new FormData(form[0]);

        var password = $("#Password").val();

        var isValid = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@@#$%^&+=]).{6,}$/.test(password);

        if (!isValid) {
            $("#createPasswordAlertMsg").removeClass('d-none');
            $("#createPasswordAlertMsg").text('Please enter a valid password.').show();
            setTimeout(function () {
                $("#createPasswordAlertMsg").text('').hide();
            }, 3000);
            return;
        }

        $.ajax({
            type: "POST",
            url: url,
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            beforeSend: showLoadingSpinner,
            success: function (response) {
                if (response.success) {
                    showAlert('createPasswordAlertMsg', 'Account created successfully0', 'fa-solid fa-check-circle');
                    window.location.href = "/user/account/login";
                } else {
                    showAlert('createPasswordAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('createPasswordAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
            },
            complete: hideLoadingSpinner
        });
    });

    // Change Password from my account
    $("#ChangePasswordFormMyAccount").submit(function (e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var formData = new FormData(form[0]);

        $.ajax({
            type: "POST",
            url: url,
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            beforeSend: showLoadingSpinner,
            success: function (response) {
                if (response.success) {
                    showAlert('accountprofilelbl', 'Password changed successfully!', 'fa-solid fa-check-circle');
                    window.location.href = "/user/account/account-profile";
                } else {
                    showAlert('accountprofilelbl', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('accountprofilelbl', response.message, 'fa-solid fa-circle-exclamation');
            },
            complete: hideLoadingSpinner
        });
    });
    // Select Frame Image
    var selectedFrame = "1";

    $('.product-frame-image').on('click', function () {
        selectedFrame = $(this).data('frame-image');
    });

    if (selectedFrame === "") {
        selectedFrame = $(".product-frame-image:first").data('frame-image');
    }

    // Frame Size
    var selectedSize = "";
    $('input[name="Size"]').change(function () {
        selectedSize = $('input[name="Size"]:checked').val();
    });

    if (selectedSize === "") {
        selectedSize = $("input[name='Size']:first").val();
    }

    var currentSelectedQuantity = "1";

    // Quantity change handler
    $(document).on("input", ".product-cart-qty-input", function () {
        currentSelectedQuantity = $(this).val();
        //cartItemQty = currentSelectedQuantity;

        if (currentSelectedQuantity < 1 || currentSelectedQuantity === "") {
            $(this).val("1"); // Set it to 1 if empty or less than 1
        }

        $(document).trigger('quantityChanged');
        //alert("Input field changed. New value: " + currentSelectedQuantity);
    });

    // Increase quantity
    $(document).on("click", ".cart-qty .inc", function () {
        var inputField = $(this).siblings(".product-cart-qty-input");
        currentSelectedQuantity = parseInt(inputField.val());
        currentSelectedQuantity = currentSelectedQuantity + 1;
        inputField.val(currentSelectedQuantity);
        //cartItemQty = currentSelectedQuantity;
        $(document).trigger('quantityChanged');
        //alert("Quantity increased. New value: " + currentSelectedQuantity);
    });

    // Decrease quantity
    $(document).on("click", ".cart-qty .dec", function () {
        var inputField = $(this).siblings(".product-cart-qty-input");
        currentSelectedQuantity = parseInt(inputField.val());
        if (currentSelectedQuantity > 1) {
            currentSelectedQuantity = currentSelectedQuantity - 1;
            inputField.val(currentSelectedQuantity);
            //cartItemQty = currentSelectedQuantity;
            $(document).trigger('quantityChanged');
            //alert("Quantity decreased. New value: " + currentSelectedQuantity);
        }
    });

    if (currentSelectedQuantity < 1) {
        currentSelectedQuantity = 1;
    }

    // Add To Cart Form
    $(".AddToCartForm").submit(function (e) {
        e.preventDefault();
        if (!userId) {
            // Display the .not-user-modal
            $('.not-user-modal').css('display', 'flex');
            $('.non-login-content').text('Please LogIn / SignUp to use all the Cart features');
            return;
        }
        var pid = $('[name="ProductID"]').val();
        var formData = new FormData();
        formData.append('ProductID', pid);
        formData.append('FrameID', selectedFrame);
        formData.append('Quantity', currentSelectedQuantity);
        formData.append('Size', selectedSize);

        $.ajax({
            type: 'POST',
            url: '/cart/AddToCart',
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    $(document).trigger('minicartUpdate');
                    $(document).trigger('cartUpdate');
                    showAlert('display-alert', 'Added To Cart', 'fa-solid fa-check-circle');
                    toggleMiniCart();
                } else {
                    showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('display-alert', 'An error occurred while processing the request. Please try again later.', 'fa-solid fa-circle-exclamation');
            }
        });
    });

    // Mini Cart
    var shippingAmount = 0;
    var cartItemTotal = 0;
    var productUrl = '/collections/artwork/';

    // Event listner for minicartUpdate
    $(document).on('minicartUpdate', function () {
        loadminiCartItems();
        //minicartCount();
    });

    loadminiCartItems();

    // Load Mini Cart Items
    function loadminiCartItems() {
        $.ajax({
            url: '/base/GetCartItems',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                var miniCartHtml = '';
                carttotalQuantity = 0;

                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    cartItemTotal += parseInt(item.ProductTbl.Price);
                    carttotalQuantity += parseInt(item.CartTbl.Quantity);
                    // Build the HTML string part by part

                    miniCartHtml += '<li class="py-2">';
                    miniCartHtml += '<div class="row align-items-center">';
                    miniCartHtml += '<div class="col-4">';
                    miniCartHtml += '<a href="' + productUrl + item.ProductTbl.Name.replace(/ /g, '-').toLowerCase() + '">';
                    miniCartHtml += '<img class="img-fluid border" src="/Content/assets/projectImages/Products/' + item.ProductTbl.Image + '" alt="...">';
                    miniCartHtml += '</a>';
                    miniCartHtml += '</div>';
                    miniCartHtml += '<div class="col-8">';
                    miniCartHtml += '<p class="mb-2">';
                    miniCartHtml += '<a class="text-mode fw-500" href="' + productUrl + item.ProductTbl.Name.replace(/ /g, '-').toLowerCase() + '">' + item.ProductTbl.Name + '</a>';
                    miniCartHtml += '<span class="m-0 text-muted w-100 d-block">Frame : ' + item.CartTbl.Size + ' " , ' + item.FrameTbl.FrameName + '</span>';
                    miniCartHtml += '<span class="m-0 text-muted w-100 d-block"> Quantity : ' + item.CartTbl.Quantity + '</span>';
                    miniCartHtml += '<input hidden type="text" name="hiddenOriginalPrice" value="' + item.ProductTbl.Price + '" />';
                    miniCartHtml += '<input hidden type="text" name="hiddenQuantity" value="' + item.CartTbl.Quantity + '" />';
                    miniCartHtml += '</p>';
                    miniCartHtml += '<div class="d-flex align-items-center">';
                    miniCartHtml += '<span class="m-0 text-muted w-100 d-block perItemTotalPriceMiniCart"><i class="fa-solid fa-indian-rupee-sign"></i></span>';
                    miniCartHtml += '<input hidden type="text" name="RemoveFromCartItem" value="' + item.CartTbl.CartID + '" />';
                    miniCartHtml += '<button type="button" class="btn btn-link small text-mode ms-auto RemoveFromCartBtn">';
                    miniCartHtml += '<i class="fa-solid fa-trash"></i> Remove';
                    miniCartHtml += '</button>';
                    miniCartHtml += '</div>';
                    miniCartHtml += '</div>';
                    miniCartHtml += '</div>';
                    miniCartHtml += '</li>';

                    // Calculate cart total and quantity
                    cartItemTotal += parseInt(item.ProductTbl.Price);
                    carttotalQuantity += parseInt(item.CartTbl.Quantity);
                }
                $('#miniCartContainer').html(miniCartHtml);

                calculateMiniCartTotal();

                minicartCount();
            },
            error: function () {
            }
        });
    }

    // Calculate Mini Cart Total
    function calculateMiniCartTotal() {

        $(".perItemTotalPriceMiniCart").each(function () {
            var $container = $(this).closest("li");
            var hiddenOriginalPrice = parseInt($container.find("input[name='hiddenOriginalPrice']").val());
            var qty = parseInt($container.find("input[name='hiddenQuantity']").val());
            var perItemTotalPriceMiniCart = hiddenOriginalPrice * qty;
            $(this).html('<i class="fa-solid fa-indian-rupee-sign"></i> ' + perItemTotalPriceMiniCart.toFixed(2));
        });

        var minicartItemTotal = 0;
        $(".perItemTotalPriceMiniCart").each(function () {
            var perItemTotalPrice = parseFloat($(this).text().replace('₹', ''));
            minicartItemTotal += perItemTotalPrice;
        });
        var minicartgrandTotal = minicartItemTotal + shippingAmount;
        $("#minicartProductTotal").html('<i class="fa-solid fa-indian-rupee-sign"></i> ' + minicartgrandTotal.toFixed(2));
    }
    calculateMiniCartTotal();

    // Remove from cart ( Mini Cart )
    $(document).on('click', '.RemoveFromCartBtn', function (e) {
        e.preventDefault();

        var $btn = $(this);
        var cartItemId = $btn.siblings('input[name="RemoveFromCartItem"]').val();

        $.ajax({
            type: 'POST',
            url: '/cart/RemoveFromCart',
            data: { cartItemId: cartItemId },
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    $btn.closest("li").remove();
                    $(document).trigger('minicartUpdate');
                    $(document).trigger('cartUpdate');
                    loadCheckoutItems();
                    calculateMiniCartTotal();

                    minicartCount();

                    showAlert('display-alert', 'Removed From Cart', 'fa-solid fa-check-circle');
                } else {
                    showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                console.log(xhr.responseText);
                console.log(textStatus);
                console.log(errorThrown);
                showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
            }
        });
    });

    // Mini Cart Item Count
    function minicartCount() {
        $.ajax({
            url: '/cart/GetCartItems',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                var cartItemscount = data.length;
                $('.your-cart-total-items').text('Your Cart Items : ( ' + cartItemscount + ' )');
                $('#cart_item_count').attr('data-cart-items', cartItemscount);
                if (cartItemscount === 0) {
                    $("#updateCart").removeClass("d-block").hide();
                    $("#calculateShippingAmount").hide();
                    $("#proceed-to-checkout-btn").addClass("disabled");
                    return;
                } else {
                    $("#updateCart").addClass("d-block").show();

                }
                $("#proceed-to-checkout-btn").on('click', function () {
                    window.location.href = "/checkout";
                });
            },
            error: function () {
                alert("An error occurred while updating the cart item count.");
            }
        });
    }

    // Cart.index.cshtml
    var cartItemQty = "";

    // Quantity change handler
    $(document).on("input", ".product-cart-qty-input-cart", function () {
        cartItemQty = $(this).val();
        cartItemQty = cartItemQty;

        if (cartItemQty < 1 || cartItemQty === "") {
            $(this).val("1"); // Set it to 1 if empty or less than 1
        }
        $(document).trigger('quantityChanged');
        //alert("cart Input field changed. New value: " + cartItemQty);
    });

    // Increase quantity
    $(document).on("click", ".cart-qty-cart .inc-cart", function () {
        var inputField = $(this).siblings(".product-cart-qty-input-cart");
        cartItemQty = parseInt(inputField.val());
        cartItemQty = cartItemQty + 1;
        inputField.val(cartItemQty);
        cartItemQty = cartItemQty;
        $(document).trigger('quantityChanged');
        //alert("cart Quantity increased. New value: " + cartItemQty);
    });

    // Decrease quantity
    $(document).on("click", ".cart-qty-cart .dec-cart", function () {
        var inputField = $(this).siblings(".product-cart-qty-input-cart");
        cartItemQty = parseInt(inputField.val());
        if (cartItemQty > 1) {
            cartItemQty = cartItemQty - 1;
            inputField.val(cartItemQty);
            cartItemQty = cartItemQty;
            $(document).trigger('quantityChanged');
            //alert("cart Quantity decreased. New value: " + cartItemQty);
        }
    });

    if (cartItemQty < 1) {
        cartItemQty = 1;
    }

    // Calculate Shipping Amount
    $('.shipping-collapse-toggle').click(function (e) {
        e.preventDefault();
        var $target = $($(this).attr('href'));

        if ($target.is(':visible')) {
            $target.hide(500);
        } else {
            $target.show(500);
        }
    });

    // cartUpdate trigger
    $(document).on('cartUpdate', function () {
        loadCartItems();
        minicartCount();
    });

    loadCartItems();

    function loadCartItems() {
        $.ajax({
            url: '/base/GetCartItems',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                var CartHtml = '';
                carttotalQuantity = 0;
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    cartItemTotal += parseInt(item.ProductTbl.Price);
                    carttotalQuantity += parseInt(item.CartTbl.Quantity);

                    CartHtml += '<div class="d-flex align-items-center flex-row w-100 pb-3 mb-3 border-bottom">';
                    CartHtml += '<a class="d-inline-block flex-shrink-0 me-3" href="' + productUrl + item.ProductTbl.Name.replace(/ /g, '-').toLowerCase() + '">';
                    CartHtml += '<img width="120" class="img-fluid" src="Content/assets/projectImages/Products/' + item.ProductTbl.Image + '" alt="...">';
                    CartHtml += '</a>';
                    CartHtml += '<div class="d-flex flex-column flex-sm-row col">';
                    CartHtml += '<div class="pe-sm-2">';
                    CartHtml += '<h3 class="product-title fs-5 mb-1">';
                    CartHtml += '<a class="text-reset" href="' + productUrl + item.ProductTbl.Name.replace(/ /g, '-').toLowerCase() + '">' + item.ProductTbl.Name + '</a>';
                    CartHtml += '</h3>';
                    CartHtml += '<div class="small"><span class="text-muted me-2">Size:</span>' + item.CartTbl.Size + ' "</div>';
                    CartHtml += '<div class="small"><span class="text-muted me-2">Frame:</span>' + item.FrameTbl.FrameName + ' </div>';
                    CartHtml += '<input hidden type="text" name="hiddenOriginalPrice" value="' + item.ProductTbl.Price + '" />';
                    CartHtml += '<div class="lead pt-1 perItemTotalPrice"><i class="fa-solid fa-indian-rupee-sign"></i> ' + item.ProductTbl.Price + '</div>';
                    CartHtml += '</div>';
                    CartHtml += '<div class="pt-2 pt-sm-0 d-flex d-sm-block ms-sm-auto">';
                    CartHtml += '<label class="form-label d-none d-sm-inline-block">Quantity</label>';
                    CartHtml += '<div class="cart-qty-cart me-3 mb-3">';
                    CartHtml += '<div class="dec-cart qty-btn-cart">-</div>';
                    CartHtml += '<input class="product-cart-qty-input-cart form-control" type="text" id="" name="cartProductItemQuantity" min="1" value="' + item.CartTbl.Quantity + '">';
                    CartHtml += '<div class="inc-cart qty-btn-cart">+</div>';
                    CartHtml += '</div>';
                    CartHtml += '<!-- Remove -->';
                    CartHtml += '<input hidden type="text" id="" name="RemoveFromCartItem" value="' + item.CartTbl.CartID + '" />';
                    CartHtml += '<button type="button" class="btn btn-link small text-mode ms-auto RemoveFromCartCartViewBtn"><i class="fa-solid fa-trash"></i> Remove</button>';
                    CartHtml += '</div>';
                    CartHtml += '</div>';
                    CartHtml += '</div>';
                }

                $('#CartContainer').html(CartHtml);
                calculategrandTotal();
            },
            error: function () {
            }
        });
    }

    // Update Cart on Update Cart Btn Click
    $(".updateCart-btn").click(function () {
        var itemsToUpdate = [];

        // Loop through each item in the cart
        $(".product-cart-qty-input-cart").each(function () {
            var cartItemId = $(this).closest(".d-flex").find("input[name='RemoveFromCartItem']").val();
            var quantity = $(this).closest(".d-flex").find("input[name='cartProductItemQuantity']").val();
            itemsToUpdate.push({ CartID: cartItemId, Quantity: quantity });
            //alert(cartItemId + " sent cartItemId");
            //alert(quantity + " sent quantity");
        });

        $.ajax({
            type: 'POST',
            url: '/cart/UpdateCart',
            data: JSON.stringify({ itemsToUpdate: itemsToUpdate }),
            processData: false,
            contentType: 'application/json', // Set the content type to JSON
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    $(document).trigger('cartUpdate');
                    $(document).trigger('minicartUpdate');
                    calculategrandTotal(); // Update totals after quantity change
                    showAlert('updatecartalert', 'Cart Updated', 'fa-solid fa-check-circle');
                } else {
                    showAlert('updatecartalert', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('updatecartalert', response.message, 'fa-solid fa-circle-exclamation');
            }
        });
    });

    // Remove cart items when the "Remove From Cart Btn" button is clicked
    $(document).on('click', '.RemoveFromCartCartViewBtn', function (e) {
        e.preventDefault();

        var $btn = $(this);
        var cartItemId = $btn.siblings('input[name="RemoveFromCartItem"]').val();

        $.ajax({
            type: 'POST',
            url: '/cart/RemoveFromCart',
            data: { cartItemId: cartItemId },
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    $btn.closest(".d-flex").remove(); // Remove the parent container
                    $(document).trigger('cartUpdate');
                    $(document).trigger('minicartUpdate');
                    minicartCount();
                    calculategrandTotal(); showAlert('display-alert', 'Removed From Cart', 'fa-solid fa-check-circle');
                } else {
                    showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                console.log(xhr.responseText);
                console.log(textStatus);
                console.log(errorThrown);
            }
        });
    });

    function calculategrandTotal() {
        shippingAmount = 0;

        $(".perItemTotalPrice").each(function () {
            var $container = $(this).closest(".d-flex");
            var hiddenOriginalPrice = parseInt($container.find("input[name='hiddenOriginalPrice']").val());
            cartItemQty = parseInt($container.find(".product-cart-qty-input-cart").val());

            var perItemTotalPrice = hiddenOriginalPrice * cartItemQty;
            $(this).html('<i class="fa-solid fa-indian-rupee-sign"></i> ' + perItemTotalPrice.toFixed(2));
        });

        cartItemTotal = 0; // Initialize the cartItemTotal to 0

        $(".perItemTotalPrice").each(function () {
            var perItemTotalPrice = parseFloat($(this).text().replace('₹', ''));
            cartItemTotal += perItemTotalPrice;
        });

        var grandTotal = cartItemTotal + shippingAmount;

        $("#shippingAmountDisplay").html('<i class="fa-solid fa-indian-rupee-sign"></i> ' + shippingAmount.toFixed(2));
        $(".productTotalDisplay").html('<i class="fa-solid fa-indian-rupee-sign"></i> ' + cartItemTotal.toFixed(2));
        $("#grandTotalDisplay").html('<i class="fa-solid fa-indian-rupee-sign"></i> ' + grandTotal.toFixed(2));
    }
    calculategrandTotal();

    $(document).on('quantityChanged', function () {
        calculategrandTotal();
        $(document).on("keypress", ".product-cart-qty-input-cart", function (e) {
            var inputField = $(this).val();
            var inputValue = inputField + String.fromCharCode(e.which);
            // Use a regex pattern to check if the input starts with '0'
            if (/^0/.test(inputValue)) {
                e.preventDefault(); // Prevent the keypress
            }
            // Check if the input starts with a zero and has more than one character
            if (inputValue.length > 1 && inputValue.charAt(0) === '0') {
                // Remove the leading zero
                inputValue = inputValue.slice(1);
                $(this).val(inputValue);
            }
        });
        $(document).on("keypress", ".product-cart-qty-input", function (e) {
            var inputField = $(this).val();
            var inputValue = inputField + String.fromCharCode(e.which);
            // Use a regex pattern to check if the input starts with '0'
            if (/^0/.test(inputValue)) {
                e.preventDefault(); // Prevent the keypress
            }
            // Check if the input starts with a zero and has more than one character
            if (inputValue.length > 1 && inputValue.charAt(0) === '0') {
                // Remove the leading zero
                inputValue = inputValue.slice(1);
                $(this).val(inputValue);
            }
        });
    });

    // Checkout.index.cshtml

    loadCheckoutItems();
    function loadCheckoutItems() {
        $.ajax({
            url: '/base/GetCartItems',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                checkoutItemCount = data.length;
                var CheckoutHTML = '';
                var productUrl = '/collections/artwork/';
                var subtotal = 0;
                var shippingCost = 0;

                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    var perItemTotal = item.CartTbl.Quantity * item.ProductTbl.Price;
                    subtotal += perItemTotal;
                    CheckoutHTML += '<li class="list-group-item checkout-list-group-item p-3">';
                    CheckoutHTML += '<div class="row g-2">';
                    CheckoutHTML += '<div class="col-3 col-md-2">';
                    CheckoutHTML += '<a href="' + productUrl + item.ProductTbl.Name.replace(/ /g, '-').toLowerCase() + '">';
                    CheckoutHTML += '<img class="img-fluid" src="Content/assets/projectImages/Products/' + item.ProductTbl.Image + '" alt="...">';
                    CheckoutHTML += '</a>';
                    CheckoutHTML += '</div>';
                    CheckoutHTML += '<div class="col">';
                    CheckoutHTML += '<div class="fw-600">';
                    CheckoutHTML += '<a class="text-mode" href="' + productUrl + item.ProductTbl.Name.replace(/ /g, '-').toLowerCase() + '">' + item.ProductTbl.Name + '</a>';
                    CheckoutHTML += '</div>';
                    CheckoutHTML += '<div class="d-flex align-items-center">';
                    CheckoutHTML += '<span class="text-body checkout-product-price">';
                    CheckoutHTML += 'Size : ' + item.CartTbl.Size + ', Frame : ' + item.FrameTbl.FrameName;
                    CheckoutHTML += '</span>';
                    CheckoutHTML += '</div>';
                    CheckoutHTML += '<div class="d-flex align-items-center">';
                    CheckoutHTML += '<span class="text-body checkout-product-price">';
                    CheckoutHTML += '<i class="fa-solid fa-indian-rupee-sign"></i> ' + item.ProductTbl.Price;
                    CheckoutHTML += '</span>';
                    CheckoutHTML += '<span class="text-body checkout-product-quantity">';
                    CheckoutHTML += '&nbsp;* ' + item.CartTbl.Quantity + ' (Qty)';
                    CheckoutHTML += '</span>';
                    CheckoutHTML += '<div class="ms-auto text-danger fs-sm">';
                    CheckoutHTML += 'Total : ';
                    CheckoutHTML += '<i class="fa-solid fa-indian-rupee-sign"></i>';
                    CheckoutHTML += '<span class="perItem-total-amt">' + perItemTotal.toFixed(2) + '</span>';
                    CheckoutHTML += '</div>';
                    CheckoutHTML += '</div>';
                    CheckoutHTML += '</div>';
                    CheckoutHTML += '</div>';
                    CheckoutHTML += '</li>';
                }

                $('#checkoutSubtotal').html('<i class="fa-solid fa-indian-rupee-sign"></i>' + subtotal.toFixed(2));
                var total = subtotal + shippingCost;
                $('#checkoutGrandTotal').html('<i class="fa-solid fa-indian-rupee-sign"></i>' + total.toFixed(2));

                $('#CheckoutContainer').html(CheckoutHTML);
            },
            error: function () {
            }
        });
    }

    // Validate Email
    function isValidEmail(email) {
        const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return emailPattern.test(email);
    }

    // Validate the Indian mobile number
    function isValidIndianMobileNumber(mobileNumber) {
        const pattern = /^[6-9]\d{9}$/;
        return pattern.test(mobileNumber);
    }

    // Validate the Indian PIN code
    function isValidIndianPINCode(pinCode) {
        const pattern = /^[1-9][0-9]{2}\s?[0-9]{3}$/;
        return pattern.test(pinCode);
    }
    function emptyBillingData() {
        $('#checkout_Billing_First_Name').val('');
        $('#checkout_Billing_Last_Name').val('');
        $('#checkout_Billing_Email').val('');
        $('#checkout_Billing_Mobile').val('');
        $('#checkout_Billing_Address').val('');
        $('#billingStatelist').val('0');
        $('#billingDistrictlist').val('0');
        $('#billingCitylist').val('0');
        $('#checkout_Billing_Town').val('');
        $('#checkout_Billing_PostCode').val('');
        $('#checkout_Billing_OrderNote').val('');
    }
    // Validate and copy shipping data to billing data
    function copyShippingToBilling() {
        const isSameAddressChecked = $('#checkoutBillingSameAddress').is(':checked');
        if (isSameAddressChecked) {
            // Copy shipping data to billing data fields
            $('#checkout_Billing_First_Name').val($('#checkout_Shipping_First_Name').val());
            $('#checkout_Billing_Last_Name').val($('#checkout_Shipping_Last_Name').val());
            $('#checkout_Billing_Email').val($('#checkout_Shipping_Email').val());
            $('#checkout_Billing_Mobile').val($('#checkout_Shipping_Mobile').val());
            $('#checkout_Billing_Address').val($('#checkout_Shipping_Address').val());
            $('#billingStatelist').val($('#statelist').val());
            $('#billingDistrictlist').val($('#districtlist').val());
            $('#billingCitylist').val($('#citylist').val());
            $('#checkout_Billing_Town').val($('#checkout_Shipping_Town').val());
            $('#checkout_Billing_PostCode').val($('#checkout_Shipping_PostCode').val());
            $('#checkout_Billing_OrderNote').val($('#checkout_Shipping_OrderNote').val());
        }
        else {
            // Clear billing data fields
            emptyBillingData();
        }
    }

    // Display error messages
    function displayErrorMessage(elementId, message) {
        const errorMessageElement = $(`#${elementId}`);
        errorMessageElement.text(message).show();
        setTimeout(function () {
            $(`#${elementId}`).text('');
        }, 1500);
    }

    // Clear error messages
    function clearErrorMessage(elementId) {
        const errorMessageElement = $(`#${elementId}`);
        errorMessageElement.text('').hide();
    }
    let ShipisValid = false;
    let BillisValid = false;
    // Shipping Address fields
    $("#statelist").change(function () {
        var selectedText = $("#statelist option:selected").text();
        $("#selectedStateText").val(selectedText);
    });

    $("#districtlist").change(function () {
        var selectedText = $("#districtlist option:selected").text();
        $("#selectedDistrictText").val(selectedText);
    });

    $("#citylist").change(function () {
        var selectedText = $("#citylist option:selected").text();
        $("#selectedCityText").val(selectedText);
    });

    // Billing Address fields
    $("#billingStatelist").change(function () {
        var selectedText = $("#billingStatelist option:selected").text();
        $("#selectedBillingStateText").val(selectedText);
    });

    $("#billingDistrictlist").change(function () {
        var selectedText = $("#billingDistrictlist option:selected").text();
        $("#selectedBillingDistrictText").val(selectedText);
    });

    $("#billingCitylist").change(function () {
        var selectedText = $("#billingCitylist option:selected").text();
        $("#selectedBillingCityText").val(selectedText);
    });


    $('#checkout-shipping-continue').click(function () {
        const shippingMobile = $('#checkout_Shipping_Mobile').val();
        const shippingPINCode = $('#checkout_Shipping_PostCode').val();

        clearErrorMessage('ShippingReqLbl');

        let isValid = true;
        function isShipFormValid() {

            if ($('#checkout_Shipping_First_Name').val() === '') {
                displayErrorMessage('FirstNameShippingReqLbl', 'First Name is required.');
                isValid = false;
            }

            if ($('#checkout_Shipping_Last_Name').val() === '') {
                displayErrorMessage('LastNameShippingReqLbl', 'Last Name is required.');
                isValid = false;
            }

            if ($('#checkout_Shipping_Email').val() === '') {
                displayErrorMessage('EmailShippingReqLbl', 'Email is required.');
                isValid = false;
            }

            if ($('#checkout_Shipping_Mobile').val() === '') {
                displayErrorMessage('MobileShippingReqLbl', 'Mobile Phone is required.');
                isValid = false;
            }

            if ($('#checkout_Shipping_Address').val() === '') {
                displayErrorMessage('AddressShippingReqLbl', 'Address is required.');
                isValid = false;
            }

            if ($('#statelist').val() === '0') {
                displayErrorMessage('statelistShippingReqLbl', 'Please select a State.');
                isValid = false;
            }

            if ($('#districtlist').val() === '0') {
                displayErrorMessage('districtlistShippingReqLbl', 'Please select a District.');
                isValid = false;
            }

            if ($('#citylist').val() === '0') {
                displayErrorMessage('citylistShippingReqLbl', 'Please select a City.');
                isValid = false;
            }

            if ($('#checkout_Shipping_Town').val() === '') {
                displayErrorMessage('TownShippingReqLbl', 'Local Town is required.');
                isValid = false;
            }

            if ($('#checkout_Shipping_PostCode').val() === '') {
                displayErrorMessage('PostCodeShippingReqLbl', 'ZIP / Postcode is required.');
                isValid = false;
            }

            if ($('#checkout_Shipping_OrderNote').val() === '') {
                displayErrorMessage('OrderNoteShippingReqLbl', 'Order Note is required.');
                isValid = false;
            }

            // Validate Indian mobile number
            if (!isValidIndianMobileNumber(shippingMobile)) {
                displayErrorMessage('MobileShippingReqLbl', 'Please enter a valid Indian mobile number.');
                isValid = false;
            }

            // Validate Indian PIN code
            if (!isValidIndianPINCode(shippingPINCode)) {
                displayErrorMessage('PostCodeShippingReqLbl', 'Please enter a valid Indian PIN code.');
                isValid = false;
            }
        }
        isShipFormValid();
        if (isValid) {
            copyShippingToBilling();

            var selectedText = $("#billingStatelist option:selected").text();
            $("#selectedBillingStateText").val(selectedText);

            var selectedText = $("#billingDistrictlist option:selected").text();
            $("#selectedBillingDistrictText").val(selectedText);

            var selectedText = $("#billingCitylist option:selected").text();
            $("#selectedBillingCityText").val(selectedText);

            $("#checkout-shipping-container").hide(400);
            setTimeout(function () {
                $("#shippingSuccessLbl").addClass('d-block');
            }, 500);
            setTimeout(function () {
                $("#billingSuccessLbl").addClass('d-block');
            }, 500);
            $("#isSameAddressDiv").addClass('d-block');
            //$("#makePayment").addClass('d-block');
            ShipisValid = true;
            BillisValid = true;
            $(document).trigger('updategocheck');
            return false;
        }
        return false;
    });

    // If the "checkoutBillingSameAddress" is checked, copy shipping data to billing data
    if ($('#checkoutBillingSameAddress').is(':checked')) {
        copyShippingToBilling();
        BillisValid = true;
        $(document).trigger('updategocheck');
    }

    $('#checkoutBillingSameAddress').change(function () {
        var $target = $('#checkout-billing-container');
        if ($(this).is(':checked')) {
            copyShippingToBilling();

            $target.hide(400);
            setTimeout(function () {
                $("#billingSuccessLbl").addClass('d-block');
            }, 500);
            BillisValid = true;
            $(document).trigger('updategocheck');
        } else {
            emptyBillingData();
            $target.show(400);
            $("#billingSuccessLbl").removeClass('d-block');
            $("#makePayment").removeClass('d-block');
            BillisValid = false;
            $(document).trigger('updategocheck');
        }
    });

    // Validate and submit the billing form when "checkout-billing-continue" is clicked

    $('#checkout-billing-continue').click(function () {
        const billingMobile = $('#checkout_Billing_Mobile').val();
        const billingPINCode = $('#checkout_Billing_PostCode').val();

        clearErrorMessage('BillingReqLbl');

        let isValid = true;
        function isBillFormValid() {

            if ($('#checkout_Billing_First_Name').val() === '') {
                displayErrorMessage('FirstNameBillingReqLbl', 'First Name is required.');
                isValid = false;
            }

            if ($('#checkout_Billing_Last_Name').val() === '') {
                displayErrorMessage('LastNameBillingReqLbl', 'Last Name is required.');
                isValid = false;
            }

            if ($('#checkout_Billing_Email').val() === '') {
                displayErrorMessage('EmailBillingReqLbl', 'Email is required.');
                isValid = false;
            }

            if ($('#checkout_Billing_Mobile').val() === '') {
                displayErrorMessage('MobileBillingReqLbl', 'Mobile Phone is required.');
                isValid = false;
            }

            if ($('#checkout_Billing_Address').val() === '') {
                displayErrorMessage('AddressBillingReqLbl', 'Address is required.');
                isValid = false;
            }

            if ($('#billingStatelist').val() === '0') {
                displayErrorMessage('statelistBillingReqLbl', 'Please select a State.');
                isValid = false;
            }

            if ($('#billingDistrictlist').val() === '0') {
                displayErrorMessage('districtlistBillingReqLbl', 'Please select a District.');
                isValid = false;
            }

            if ($('#billingCitylist').val() === '0') {
                displayErrorMessage('citylistBillingReqLbl', 'Please select a City.');
                isValid = false;
            }

            if ($('#checkout_Billing_Town').val() === '') {
                displayErrorMessage('TownBillingReqLbl', 'Local Town is required.');
                isValid = false;
            }

            if ($('#checkout_Billing_PostCode').val() === '') {
                displayErrorMessage('PostCodeBillingReqLbl', 'ZIP / Postcode is required.');
                isValid = false;
            }

            if ($('#checkout_Billing_OrderNote').val() === '') {
                displayErrorMessage('OrderNoteBillingReqLbl', 'Order Note is required.');
                isValid = false;
            }

            // Validate Indian mobile number
            if (!isValidIndianMobileNumber(billingMobile)) {
                displayErrorMessage('MobileBillingReqLbl', 'Please enter a valid Indian mobile number.');
                isValid = false;
            }

            // Validate Indian PIN code
            if (!isValidIndianPINCode(billingPINCode)) {
                displayErrorMessage('PostCodeBillingReqLbl', 'Please enter a valid Indian PIN code.');
                isValid = false;
            }
        }
        isBillFormValid();
        if (isValid) {
            $("#billingSuccessLbl").addClass('d-block');
            $("#checkout-billing-container").hide(400);
            BillisValid = true;
            $(document).trigger('updategocheck');
            return false;
        }
        return false;
    });
    // Initially disable the "Make Payment" button
    $("#makePayment").prop('disabled', true);

    // Your existing code ...

    // Event listner for gocheck
    $(document).on('updategocheck', function () {
        gocheck();
    });

    // Inside your event handlers where ShipisValid and BillisValid are updated:
    function gocheck() {
        if (ShipisValid === true && BillisValid === true) {
            //alert("ShipisValid " + ShipisValid);
            //alert("BillisValid " + BillisValid);
            $("#makePayment").prop('disabled', false); // Enable the button
        } else {
            $("#makePayment").prop('disabled', true); // Disable the button
            //alert("ShipisValid " + ShipisValid);
            //alert("BillisValid " + BillisValid);
        }
    }
    gocheck();
    // Make Payment 
    $("#InitiateOrderForm").submit(function (e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var formData = new FormData(form[0]);
        //var formData = $("#InitiateOrderForm").serialize();

        console.log(formData);

        $.ajax({
            type: "POST",
            url: "/checkout/CreateCheckoutSessions",
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    //alert("Order success");
                    window.location.href = response.redirectUrl; // Redirect to the specified URL

                } else {
                    showAlert('display-alert', "Order failed: " + response.errorMessage, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function (xhr, status, error) {
                showAlert('display-alert', "AJAX error: " + error, 'fa-solid fa-circle-exclamation');
            }
        });
    });

    // Call updateTotalCountLabel on page load
    updateTotalCountLabel();

    // Function to filter arts by price range
    function filterArtsByPrice() {
        // Get the selected price range checkboxes
        var selectedRanges = $("input.custom-control-input:checked");

        // Get the min and max prices from the input fields
        var minPrice = parseFloat($("#minPrice").val());
        var maxPrice = parseFloat($("#maxPrice").val());

        // Check if custom min and max prices are entered
        var customPriceFilterApplied = !isNaN(minPrice) || !isNaN(maxPrice);

        // Hide all art items initially
        $(".art-item").hide();

        if (customPriceFilterApplied) {
            // Clear all checkboxes
            $("input.custom-control-input").prop("checked", false);

            // Show art items within the specified custom price range
            $(".art-item").each(function () {
                var price = parseFloat($(this).data("price"));
                if (!isNaN(price) && price >= minPrice && price <= maxPrice) {
                    $(this).show();
                }
            });
        } else if (selectedRanges.length > 0) {
            // Iterate through selected price ranges and show matching art items
            selectedRanges.each(function () {
                var min = parseFloat($(this).data("min"));
                var max = parseFloat($(this).data("max"));

                $(".art-item").each(function () {
                    var price = parseFloat($(this).data("price"));
                    if (!isNaN(price) && price >= min && price <= max) {
                        $(this).show();
                    }
                });
            });
        } else {
            // If no filters are applied, show all art items
            $(".art-item").show();
        }

        // Update the total count label
        updateTotalCountLabel();
        // Update the visibility of the "Clear All Filters" button
        updateClearFiltersButtonVisibility();
    }

    // Function to update the visibility of the "Clear All Filters" button
    function updateClearFiltersButtonVisibility() {
        // Check if any price range checkbox is checked
        var anyChecked = $("input.custom-control-input:checked").length > 0;

        // Check if min and max price fields have values
        var minPrice = parseFloat($("#minPrice").val());
        var maxPrice = parseFloat($("#maxPrice").val());
        var priceFilterApplied = !isNaN(minPrice) || !isNaN(maxPrice);

        // Check if any sorting criteria is applied
        var sortingCriteria = $("#sort-select").val();
        var sortingApplied = sortingCriteria !== "default-sort";

        // Show or hide the "Clear All Filters" button based on filter and sorting status
        if (anyChecked || priceFilterApplied || sortingApplied) {
            $("#remove-all-filter-btn").show();
        } else {
            $("#remove-all-filter-btn").hide();
        }
    }

    // Function to update the total count label
    function updateTotalCountLabel() {
        var visibleArtItemsCount = $(".art-item:visible").length;
        var totalCountLabel = "Total " + visibleArtItemsCount + " listings";
        $(".total-product-count-display-lbl").text(totalCountLabel);
    }

    // Attach event handlers
    $("input[type='checkbox'], #minPrice, #maxPrice").change(function () {
        filterArtsByPrice();
        // Check if any price range checkbox is checked
        var anyChecked = $("input.custom-control-input:checked").length > 0;

        // If at least one checkbox is checked, hide the offcanvas menu
        if (anyChecked) {
            $("#shop_filter").offcanvas("hide");
        }
    });

    $("#applyPriceFilter").click(function () {
        filterArtsByPrice();
    });

    // Function to sort the products
    function sortProducts(criteria) {
        const productList = $(".art-item").clone().toArray();
        if (criteria === "price-asc") {
            productList.sort((a, b) => {
                const valueA = $(a).data("price");
                const valueB = $(b).data("price");
                return valueA - valueB;
            });
        } else if (criteria === "price-desc") {
            productList.sort((a, b) => {
                const valueA = $(a).data("price");
                const valueB = $(b).data("price");
                return valueB - valueA;
            });
        } else if (criteria === "name-asc") {
            productList.sort((a, b) => {
                const valueA = $(a).data("name");
                const valueB = $(b).data("name");
                return valueA.localeCompare(valueB);
            });
        } else if (criteria === "name-desc") {
            productList.sort((a, b) => {
                const valueA = $(a).data("name");
                const valueB = $(b).data("name");
                return valueB.localeCompare(valueA);
            });
        }
        $(".art-item").remove();
        $("#product-container").append(productList);
    }

    // Function to sort the products

    // Attach a change event handler to the select element for sorting
    $("#sort-select").change(function () {
        var sortingCriteria = $(this).val();
        sortProducts(sortingCriteria);
        updateClearFiltersButtonVisibility();
    });

    // Initialize the sorting based on the default selected option
    var defaultSortingCriteria = $("#sort-select").val();
    sortProducts(defaultSortingCriteria);

    // Attach a click event handler to the "Clear All Filters" button
    $("#remove-all-filter-btn").click(function () {
        // Clear all filters and sorting
        $("input.custom-control-input").prop("checked", false);
        $("#minPrice").val("");
        $("#maxPrice").val("");
        $("#sort-select").val("SortBy");
        // Show all art items
        $(".art-item").show();
        // Hide the "Clear All Filters" button
        $(this).hide();
        // Update the total count label
        updateTotalCountLabel();
    });

    // search auto suggestions
    var searchInput = $("#searchInput");
    var suggestionList = $("#search-autocomplete");
    var minCharacters = 2; // Minimum characters required for autocomplete

    searchInput.on("input", function () {
        var query = searchInput.val();

        if (query.length >= minCharacters) {
            suggestionList.hide().empty();

            $.ajax({
                url: "/Home/GetAutoSuggestions",
                type: "POST",
                data: { query: query },
                dataType: "json",
                success: function (data) {
                    if (data.length > 0) {
                        $.each(data, function (index, item) {
                            var listItem = $("<li class='search-autocomplete-li list-unstyled'>");
                            var linkItem = $("<a>").text(item.value).addClass("search-autocomplete-item nav-link py-2 px-3");
                            linkItem.attr("href", "/collections/artwork/" + item.value.replace(/ /g, '-').toLowerCase());
                            listItem.append(linkItem);

                            linkItem.on("click", function () {
                                searchInput.val(item.value);
                                suggestionList.hide();
                            });

                            suggestionList.append(listItem);
                        });

                        suggestionList.show();

                    } else {
                        suggestionList.hide();
                    }
                }
            });
        } else {
            suggestionList.hide().empty();
        }
    });

    // update accountprofile
    $("#updateaccountprofile").submit(function (e) {
        e.preventDefault();

        var Mobile = $('[name="Mobile"]').val();
        if (!isValidIndianMobileNumber(Mobile)) {
            $('#accountprofilelbl').text('Please enter a valid Indian mobile number.');
            setTimeout(function () {
                $('#accountprofilelbl').text('');
            }, 2000);
            return;
        }

        var form = $(this);
        var url = form.attr('action');
        var formData = new FormData(form[0]);

        $.ajax({
            type: 'POST',
            url: url,
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            success: function (response) {
                if (response.success) {

                    $('#accountprofilelbl').text('Profile update successfully!');
                    setTimeout(function () {
                        $('#accountprofilelbl').text('');
                    }, 2000);

                } else {
                    $('#accountprofilelbl').text(response.message);
                    setTimeout(function () {
                        $('#accountprofilelbl').text('');
                    }, 2000);
                }
            },
            error: function () {
                $('#accountprofilelbl').text('An error occurred while processing the request. Please try again later.');
                setTimeout(function () {
                    $('#accountprofilelbl').text('');
                }, 2000);
            }
        });
    });

    // Toggle Mini Cart
    function toggleMiniCart() {
        $('#modalMiniCart').offcanvas('toggle');
    }

    // Close the not-user-modal with animation
    $('.modal-close-icon').click(function () {
        $('.not-user-modal').fadeOut(50);
    });

    // function to populate dropdown based on URL and target dropdown ID for StateList, DistrictList, CityList
    function populateDropdown(url, targetId) {
        $.getJSON(url, function (data) {
            var dropdown = $("." + targetId);
            dropdown.empty();

            var newOption1 = $("<option>").html("Select " + targetId.replace("list", "")).val("0");
            dropdown.append(newOption1);

            $.each(data, function (i, item) {
                var newOption = $("<option>").html(item.Text).val(item.Value);
                dropdown.append(newOption);
            });
        });
    }

    // Populate the state dropdown on page load
    populateDropdown('/cart/statelist', 'statelist');

    // Populate the district dropdown on statelist change
    $('.statelist').change(function () {
        populateDropdown('/cart/districtlist/' + $(this).val(), 'districtlist');
    });

    // Populate the city dropdown on districtlist change
    $('.districtlist').change(function () {
        populateDropdown('/cart/citylist/' + $(this).val(), 'citylist');
    });

    // Add review form
    $("#AddReviewForm").submit(function (e) {
        e.preventDefault();

        var form = $(this);
        var url = form.attr('action');
        var formData = new FormData(form[0]);

        var title = $("input[name='title']").val();
        var comment = $("textarea[name='comment']").val();
        var username = $("input[name='username']").val();
        var useremail = $("input[name='useremail']").val();

        $("#titleReqAlertMsg").text('');
        $("#commentReqAlertMsg").text('');
        $("#usernameReqAlertMsg").text('');
        $("#useremailReqAlertMsg").text('');

        var isValid = true;

        if (title.trim() === "") {
            $("#titleReqAlertMsg").text('Please enter a title.').show();
            isValid = false;
        }

        if (comment.trim() === "") {
            $("#commentReqAlertMsg").text('Please write your comments.').show();
            isValid = false;
        }

        if (username.trim() === "") {
            $("#usernameReqAlertMsg").text('Please enter your name.').show();
            isValid = false;
        }

        if (useremail.trim() === "") {
            $("#useremailReqAlertMsg").text('Please enter your email.').show();
            isValid = false;
        }

        if (!isValid) {
            setTimeout(function () {
                $("#titleReqAlertMsg").text('');
                $("#commentReqAlertMsg").text('');
                $("#usernameReqAlertMsg").text('');
                $("#useremailReqAlertMsg").text('');
            }, 5000);
            return;
        }
        $.ajax({
            type: "POST",
            url: url,
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    $('#ReqAlertMsg').text('Review added successfully !');
                    setTimeout(function () {
                        $('#ReqAlertMsg').text('');
                    }, 5000);
                }
                else {
                    $('#ReqAlertMsg').text('An error occured. Please wait and try again later.');
                    setTimeout(function () {
                        $('#ReqAlertMsg').text('');
                    }, 5000);
                }
            },
            error: function () {
                $('#ReqAlertMsg').text('An error occurred while processing the request. Please try again later.');
                setTimeout(function () {
                    $('#ReqAlertMsg').text('');
                }, 5000);
            }
        });
    });

    // Ask about product form
    $("#AskAboutProductForm").submit(function (e) {
        e.preventDefault();

        var form = $(this);
        var url = form.attr('action');
        var formData = new FormData(form[0]);

        var uname = $("input[name='uname']").val();
        var uemail = $("input[name='uemail']").val();
        var usub = $("input[name='usub']").val();
        var umsg = $("textarea[name='umsg']").val();

        // Reset all validation messages
        $("#unameReqAlertMsg").text('');
        $("#uemailReqAlertMsg").text('');
        $("#usubReqAlertMsg").text('');
        $("#umsgReqAlertMsg").text('');

        var isValid = true; // A flag to track overall form validity

        if (uname.trim() === "") {
            $("#unameReqAlertMsg").text('Please enter your name.').show();
            isValid = false;
        }

        if (uemail.trim() === "") {
            $("#uemailReqAlertMsg").text('Please write your email.').show();
            isValid = false;
        }

        if (usub.trim() === "") {
            $("#usubReqAlertMsg").text('Please enter your subject.').show();
            isValid = false;
        }

        if (umsg.trim() === "") {
            $("#umsgReqAlertMsg").text('Please enter your message.').show();
            isValid = false;
        }

        if (!isValid) {
            setTimeout(function () {
                $("#unameReqAlertMsg").text('');
                $("#uemailReqAlertMsg").text('');
                $("#usubReqAlertMsg").text('');
                $("#umsgReqAlertMsg").text('');
            }, 5000);
            return; // Exit the function without making the AJAX request
        }

        $.ajax({
            type: "POST",
            url: url,
            data: formData, // Use FormData for file uploads
            processData: false, // Prevent jQuery from processing the data
            contentType: false,
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    //window.location.href = response.redirectUrl;
                    $('#AskAboutProductFormReqAlertMsg').text('Enquirey sent successfully !');
                    setTimeout(function () {
                        $('#AskAboutProductFormReqAlertMsg').text('');
                    }, 5000);
                } else {
                    $('#AskAboutProductFormReqAlertMsg').text('An error occured. Please wait and try again later.');
                    setTimeout(function () {
                        $('#AskAboutProductFormReqAlertMsg').text('');
                    }, 5000);
                }
            },
            error: function () {
                // Show error message for AJAX error
                $('#AskAboutProductFormReqAlertMsg').text('An error occurred while processing the request. Please try again later.');

                setTimeout(function () {
                    $('#AskAboutProductFormReqAlertMsg').text('');
                }, 5000);
            }
        });
    });

    // Login - login.cshtml
    $("#login").submit(function (e) {
        e.preventDefault();
        var email = $("#LoginEmail").val();
        var password = $("#LoginPassword").val();
        var form = $(this);
        var url = form.attr('action');
        $.ajax({
            type: 'POST',
            url: url,
            data: { Email: email, Password: password },
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    showAlert('alertlbl', 'Login successful!', 'fa-solid fa-check-circle');
                    window.location.href = "/";
                } else {
                    showAlert('alertlbl', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('alertlbl', 'An error occurred while processing the request. Please try again later.', 'fa-solid fa-circle-exclamation');
            }
        });
    });

    // Login - Home Login Model
    $("#HomeModellogin").submit(function (e) {
        e.preventDefault();
        var email = $("#EmailHomeModel").val();
        var password = $("#PasswordHomeModel").val();
        var form = $(this);
        var url = form.attr('action');
        $.ajax({
            type: "POST",
            url: url,
            data: { Email: email, Password: password },
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    showAlert('loginAlertMsg', 'Login successful!', 'fa-solid fa-check-circle');
                    window.location.href = "/";
                } else {
                    showAlert('loginAlertMsg', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('loginAlertMsg', 'An error occurred while processing the request. Please try again later.', 'fa-solid fa-circle-exclamation');
            }
        });
    });

    // go-to-login-btnlink
    $("#go-to-login-btnlink").on('click', function () {
        window.location.href = "/user/account/login";
    });

    // go-to-signup-btnlink
    $("#go-to-signup-btnlink").on('click', function () {
        window.location.href = "/user/account/signup";
    });

    // continue to checkout btn
    $(".continue-to-checkout-btn").on('click', function () {
        if (!userId) {
            // Display the .not-user-modal
            $('.not-user-modal').css('display', 'flex');
            $('.non-login-content').text('Please LogIn / SignUp to use all the Checkout features');
            return;
        }
        window.location.href = "/checkout";
    });

    // continue to view cart btn
    $(".continue-to-view-cart-btn").on('click', function () {
        if (!userId) {
            // Display the .not-user-modal
            $('.not-user-modal').css('display', 'flex');
            $('.non-login-content').text('Please LogIn / SignUp to use all the Cart features');
            return;
        }
        window.location.href = "/cart";
    });
    // My Orders Count
    function myOrdersCount() {
        $.ajax({
            url: '/base/GetMyOrdersCount',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $('#myTotalOrdersCount').text(data);
            },
            error: function () {
                console.log("An error occurred while getting the wishlist items.");
            }
        });
    }
    myOrdersCount();

    // Wishlist Item Count
    function WishlistCount() {
        $.ajax({
            url: '/base/GetWishlistItemsCount',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $('#wishlist-count').attr('data-wishlist-count', data);

            },
            error: function () {
                console.log("An error occurred while getting the wishlist items.");
            }
        });
    }
    WishlistCount();

    //add to wishlist
    $(".add-to-wishlist-btn").on('click', function () {
        if (!userId) {
            // Display the .not-user-modal
            $('.not-user-modal').css('display', 'flex');
            return;
        }
        var pid = $(this).closest('.data-product-id-container-div').data('product-id');
        if (typeof pid === "undefined") {
            // If pid is undefined, try the second method
            pid = $(this).closest('form').find('[name="ProductID"]').val();
        }
        var formData = new FormData();
        formData.append('id', pid);
        $.ajax({
            type: 'POST',
            url: '/wishlist/AddToWishlist',
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    WishlistCount();
                    showAlert('display-alert', 'Added to wishlist', 'fa-solid fa-check-circle');
                } else {
                    showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
            }
        });
    });

    //remove from wishlist
    $(".remove-from-wishlist-btn").on('click', function () {
        var WishlistID = $(this).closest('.data-product-id-container-div').data('wishlist-id');
        $.ajax({
            type: 'POST',
            url: '/Wishlist/RemoveFromWishlist',
            data: { WishlistID: WishlistID },
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    WishlistCount();
                    showAlert('display-alert', 'Removed From Wishlist', 'fa-solid fa-check-circle');
                    window.location.href = "/wishlist";
                } else {
                    showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function () {
                showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
            }
        });
    });

    // direct add to cart on button click
    $('.direct-add-to-cart-btn').on('click', function () {
        if (!userId) {
            // Display the .not-user-modal
            $('.not-user-modal').css('display', 'flex');
            $('.non-login-content').text('Please LogIn / SignUp to use all the Cart features');
            return;
        }
        var pid = $(this).closest('.data-product-id-container-div').data('product-id');
        var formData = new FormData();
        formData.append('ProductID', pid);
        formData.append('FrameID', 1);
        formData.append('Quantity', 1);
        formData.append('Size', 6);
        function toggleMiniCart() {
            $('#modalMiniCart').offcanvas('toggle');
        }

        $.ajax({
            type: 'POST',
            url: '/cart/AddToCart',
            data: formData,
            processData: false,
            contentType: false,
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    $(document).trigger('minicartUpdate');
                    $(document).trigger('cartUpdate');
                    setTimeout(toggleMiniCart, 500);
                    showAlert('display-alert', 'Added To Cart', 'fa-solid fa-check-circle');

                } else {
                    showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
                }
            },
            error: function (xhr, status, error) {
                showAlert('display-alert', response.message, 'fa-solid fa-circle-exclamation');
            }
        });
    });

    $("#generate-invoice").click(function () {
        // Redirect to the GenerateInvoicePdf action to generate and download the PDF
        window.location.href = "https://deeptiarts.databoltahai.in/checkout/order-placed/invoice";
    });
});