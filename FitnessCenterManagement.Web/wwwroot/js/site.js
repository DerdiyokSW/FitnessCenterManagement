/* ========================================
   Spor Salonu Yönetim Sistemi - JavaScript
   ======================================== */

// ========================================
// Sayfa Yüklendiğinde Çalışacak Fonksiyonlar
// ========================================
document.addEventListener('DOMContentLoaded', function() {
    // Tooltip'leri Başlatma
    initTooltips();
    
    // Form Validasyonunu Başlatma
    initFormValidation();
    
    // Bootstrap Alert Otomatik Kapat
    autoCloseAlerts();
});

// ========================================
// Bootstrap Tooltip'lerini Başlatma
// ========================================
function initTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// ========================================
// Bootstrap Popover'larını Başlatma
// ========================================
function initPopovers() {
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function(popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// ========================================
// Form Validasyonu
// ========================================
function initFormValidation() {
    // Bootstrap Validasyonu
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
}

// ========================================
// Alert Mesajlarını Otomatik Kapat (5 saniye)
// ========================================
function autoCloseAlerts() {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        if (alert.classList.contains('auto-close')) {
            setTimeout(() => {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }, 5000); // 5 saniye sonra kapat
        }
    });
}

// ========================================
// Randevu Çakışması Kontrolü
// ========================================
function kontrolEtRandevuCakismasi() {
    const startTime = document.getElementById('BaslamaTarihi');
    const endTime = document.getElementById('BitisTarihi');
    
    if (startTime && endTime) {
        // Bitiş tarihi >= Başlama tarihi kontrolü
        if (new Date(endTime.value) <= new Date(startTime.value)) {
            alert('Bitiş tarihi başlama tarihinden sonra olmalıdır!');
            endTime.value = '';
            return false;
        }
    }
    return true;
}

// ========================================
// Onay Dialoqusu Göster
// ========================================
function silmeOnayı(itemAdi) {
    return confirm(`"${itemAdi}" öğesini silmek istediğinizden emin misiniz?`);
}

// ========================================
// Tarihi Formatla (TR)
// ========================================
function tarihiniBiçimlendir(tarih) {
    const options = { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    };
    return new Date(tarih).toLocaleDateString('tr-TR', options);
}

// ========================================
// Sayı Formatı (Türk Lokalizasyonu)
// ========================================
function sayiBiçimlendir(sayi) {
    return new Intl.NumberFormat('tr-TR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(sayi);
}

// ========================================
// Para Formatı (₺)
// ========================================
function paraBiçimlendir(tutar) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(tutar);
}

// ========================================
// AJAX isteği gönder (Hata işleme ile)
// ========================================
function ajaxIstegiGonder(url, method = 'GET') {
    return fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin'
    });
}

// ========================================
// API ile Antrenör Müsaitliğini Kontrol Et
// ========================================
function antrenorMusaitlikKontrol(antrenorId, tarih) {
    const url = `/api/antrenorler/available?date=${tarih}`;
    
    ajaxIstegiGonder(url)
        .then(response => response.json())
        .then(data => {
            const musaitler = data.map(a => a.id);
            if (musaitler.includes(parseInt(antrenorId))) {
                console.log('Antrenör müsait');
                return true;
            } else {
                console.log('Antrenör o tarihte müsait değil');
                return false;
            }
        })
        .catch(error => console.error('Hata:', error));
}

// ========================================
// Uyarı Mesajı Göster
// ========================================
function uyariGoster(baslik, mesaj, tip = 'info') {
    const alertHTML = `
        <div class="alert alert-${tip} alert-dismissible fade show" role="alert">
            <strong>${baslik}:</strong> ${mesaj}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    const container = document.querySelector('.container');
    if (container) {
        container.insertAdjacentHTML('afterbegin', alertHTML);
    }
}

// ========================================
// Sayfa Gösterme/Gizleme (Loading)
// ========================================
function yukleniyorGoster(goster = true) {
    const spinner = document.getElementById('loadingSpinner');
    if (spinner) {
        spinner.style.display = goster ? 'block' : 'none';
    }
}

