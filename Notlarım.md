# 🤖 Makine Öğrenmesi Eğitim Algoritmaları Üzerine Notlarım (Finansal)📝

Selamlar! Bu dökümanda, projelerimde karşılaştığım ve kullandığım eğitim algoritmalarını, net ve teknik bir dille, kendimden emin bir şekilde açıklıyorum. GitHub profilimde referans olarak kalacak.

---

## Tree Ensemble Methods 🌳🌲

Bu bölümde, birden fazla karar ağacını birleştirerek isabetli tahminler yapan topluluk yöntemlerini ele alıyoruz.

### 1. `FastForest`

* **Nedir?**: Bu, **Random Forest** (Rastgele Orman) algoritmasının yüksek performans için optimize edilmiş bir uygulamasıdır.
* **Nasıl Çalışır?**: Çok sayıda karar ağacını, verinin ve özniteliklerin rastgele alt kümeleriyle (*bagging* ve *feature bagging*) bağımsız olarak eğitir. Tahminleri birleştirirken sınıflandırma için çoğunluk oylaması, regresyon için ortalama alır.
* **"Fast" Kısmı**: Adındaki "Fast", arka planda kullanılan paralelleştirme, verimli bellek yönetimi ve hızlı düğüm bölme (node splitting) gibi performans iyileştirmelerinden gelir.

### 2. `FastTree`

* **Nedir?**: Bu da hızlandırılmış bir ağaç topluluğu algoritmasıdır ve **Gradient Boosting** (Gradyan Artırma) veya MART (Multiple Additive Regression Trees) mantığı üzerine kuruludur.
* **Nasıl Çalışır?**: Ağaçlar ardışık olarak eğitilir; her yeni ağaç, önceki ağaçların kolektif olarak yaptığı hataları (residual errors veya gradients) minimize etmeye odaklanır ve modeli adım adım güçlendirir.
* **"Fast" Kısmı**: Hızını, histogram tabanlı bölme (histogram-based splitting), verimli önbellekleme (efficient caching) ve seyrek veri optimizasyonları gibi modern tekniklere borçludur.

### 3. `LightGbm` (Light Gradient Boosting Machine)

* **Nedir?**: Gradient Boosting Decision Tree (GBDT) ailesinin en verimli ve popüler üyelerinden biridir. Yüksek hızı ve doğruluğu ile bilinir, özellikle büyük veri setlerinde fark yaratır.
* **Anahtar Teknikleri**:
    * **GOSS (Gradient-based One-Side Sampling)**: Büyük gradyanlı (yani daha fazla hata içeren) örneklere odaklanıp, küçük gradyanlıları akıllıca örnekleyerek eğitimi hızlandırır.
    * **EFB (Exclusive Feature Bundling)**: Birbirini dışlayan öznitelikleri (genellikle seyrek verilerde görülür) tek bir öznitelikmiş gibi ele alarak boyut azaltır ve hesaplama maliyetini düşürür.
    * **Histogram Tabanlı Algoritmalar**: Sürekli öznitelikleri ayrık bölmelere (bins) ayırarak en uygun bölünme noktasını bulma işlemini dramatik şekilde hızlandırır.
    * **Yaprak Bazlı Büyüme (Leaf-wise Growth)**: Ağacı katman katman değil, kayıpta en büyük azalmayı sağlayan yapraktan (leaf) büyüterek daha optimize edilmiş ağaçlar oluşturur.

---

## Linear Models & Optimizations 📏⚙️

Doğrusal ilişkileri modelleyen ve bunları verimli şekilde optimize eden algoritmalara odaklanıyoruz.

### 4. `AveragedPerceptron`

* **Nedir?**: Standart **Perceptron** algoritmasının, kararlılığı ve genelleme yeteneği artırılmış bir varyasyonudur.
* **Nasıl Çalışır?**: Eğitim boyunca her adımda (veya her güncellemede) oluşan ağırlık vektörlerinin ortalamasını alarak nihai modeli oluşturur. Bu ortalama işlemi, modelin veri setindeki gürültüye karşı daha dayanıklı olmasını sağlar.

### 5. `LbfgsLogisticRegression`

* **Nedir?**: **Lojistik Regresyon** modelini (olasılıksal doğrusal sınıflandırıcı) eğitmek için **Limited-memory BFGS (L-BFGS)** optimizasyon algoritmasını kullanan bir yöntemdir.
* **L-BFGS'in Gücü**: L-BFGS, ikinci türev bilgilerini (Hessian matrisi) doğrudan hesaplamadan, gradyan geçmişini kullanarak bu bilgiyi yaklaşık olarak tahmin eden bir quasi-Newton yöntemidir. Bu sayede, özellikle yüksek boyutlu öznitelik uzaylarında Lojistik Regresyon'u bellek ve hesaplama açısından çok verimli bir şekilde optimize eder.

### 6. `LdSvm` (Linear Dual Support Vector Machine)

* **Nedir?**: Doğrusal bir çekirdek (kernel) kullanan **Destek Vektör Makinesi (SVM)** sınıflandırıcısıdır ve optimizasyon için SVM'nin **ikili (dual) problemini** çözer.
* **Avantajı**: İkili formülasyon, çözümün yalnızca destek vektörlerine bağlı olması nedeniyle, öznitelik sayısının veri noktası sayısından fazla olduğu (yüksek boyutlu, seyrek) durumlarda veya doğrusal ayrımın yeterli olduğu problemlerde birincil (primal) formülasyona göre daha verimli çalışır.

### 7. `LinearSvm`

* **Nedir?**: Doğrusal **Destek Vektör Makinesi (SVM)** sınıflandırıcısıdır. Amacı, farklı sınıflara ait veri noktaları arasındaki **marjini (margin) maksimize eden** bir hiper-düzlem (hyperplane) bulmaktır.
* **Nasıl Çalışır?**: `Hinge loss` kayıp fonksiyonunu (genellikle L1 veya L2 regülarizasyonu ile birlikte) minimize ederek bu hiper-düzlemi bulur. Doğrusal olarak ayrılabilir veya buna yakın veri setleri ve yüksek boyutlu öznitelik uzayları için etkili ve hızlı bir çözümdür.

### 8. `SdcaLogisticRegression`

* **Nedir?**: Lojistik Regresyon modelini, **Stokastik İkili Koordinat Yükselişi (SDCA)** optimizasyon algoritması ile eğitir.
* **SDCA'nın Yaklaşımı**: Bu algoritma, Lojistik Regresyon'un (regülarizasyonlu) ikili (dual) optimizasyon problemini çözer. Her iterasyonda rastgele bir veri noktası seçer ve bu noktaya karşılık gelen ikili değişkeni (dual variable) optimize ederek ikili amaç fonksiyonunu maksimize eder. Büyük ölçekli ve seyrek (sparse) veriler için çok hızlı yakınsama sağlayan etkin bir yöntemdir.

### 9. `SdcaNonCalibrated`

* **Nedir?**: SDCA kullanılarak eğitilmiş bir modeldir (Lojistik Regresyon veya Doğrusal SVM), ancak çıktısı **kalibre edilmemiştir**.
* **Anlamı**: Modelin ürettiği ham skorlar (örneğin, `w^T x + b` değeri) doğrudan olasılık olarak yorumlanamaz. Sıralama veya bir eşik değere göre karar verme için kullanılabilir, ancak güvenilir olasılık tahminleri için ek bir kalibrasyon adımına (Platt scaling, isotonic regression vb.) ihtiyaç duyar.

### 10. `SgdCalibrated`

* **Nedir?**: Modeli (genellikle doğrusal bir model) **Stokastik Gradyan İniş (SGD)** ile eğitir ve ardından çıktılarını **kalibre eder**.
* **SGD + Kalibrasyon**: SGD, her adımda küçük veri grupları (mini-batch) veya tekil örnekler üzerinden gradyanı hesaplayarak modeli güncelleyen, büyük veri setleri için ideal bir optimizasyon algoritmasıdır. "Calibrated" ifadesi, SGD ile elde edilen ham model çıktılarının, daha doğru olasılık tahminleri sağlamak amacıyla (örneğin Platt scaling veya isotonic regression ile) bir kalibrasyon işleminden geçirildiğini belirtir.

### 11. `SymbolicSgdLogisticRegression`

* **Nedir?**: SGD optimizasyonu kullanılarak eğitilen bir Lojistik Regresyon modelidir.
* **"Symbolic" Kısmı**: Buradaki "Symbolic", gradyan hesaplamalarının veya model yapısının, sayısal optimizasyon sırasında **sembolik matematik** araçları kullanılarak ifade edildiğini veya işlendiğini gösterir. Bu, otomatik farklılaşma (automatic differentiation) sağlar ve potansiyel derleme zamanı optimizasyonlarına olanak tanır.

---

## Other Methods ✨

Farklı yaklaşımlara sahip diğer algoritmalara da değinelim.

### 12. `FieldAwareFactorizationMachine` (FAFM)

* **Nedir?**: Bu, **Faktörizasyon Makineleri**'nin (FM) **alan (field)** bilgisini dikkate alarak geliştirilmiş bir versiyonudur. Özellikle yüksek boyutlu ve seyrek kategorik verilerin (örneğin, online reklamcılıkta tıklama oranı tahmini) modellenmesinde son derece etkilidir.
* **Önemli Farkı**: Standart FM, tüm öznitelik çiftlerinin etkileşimini tek tip bir gizli vektör (latent vector) temsili üzerinden modeller. FAFM ise, her özniteliğin, etkileşime girdiği diğer özniteliğin **alanına (field)** özgü ayrı bir gizli vektörü olmasını sağlar (`v_{j,f_k}`). Bu sayede, öznitelikler arasındaki etkileşimleri (`<v_{j,f_k}, v_{k,f_j}>`) çok daha zengin ve bağlama duyarlı bir şekilde modeller.

### 13. `Gam` (Generalized Additive Model)

* **Nedir?**: Genelleştirilmiş Doğrusal Modelleri (GLM), özniteliklerin doğrusal olmayan etkilerini yakalayacak şekilde genişleten yarı-parametrik bir modeldir.
* **Nasıl Çalışır?**: Hedef değişkenin beklenen değerini, bir bağlantı fonksiyonu (`g`) aracılığıyla, özniteliklerin **düzgün (smooth) fonksiyonlarının (`s_i`)** toplamına bağlar: `g(E[Y|X]) = \alpha + \sum_{i=1}^{p} s_i(X_i)`. Bu `s_i` fonksiyonları (genellikle spline'lar kullanılarak tahmin edilir) her bir özniteliğin hedef üzerindeki potansiyel eğrisel etkisini esnek bir şekilde modellemeye olanak tanır. Böylece hem doğrusal olmayan ilişkiler yakalanır hem de her özniteliğin katkısı yorumlanabilir kalır.

### 14. `Prior`

* **Nedir?**: En temel **baseline** (referans noktası) modelidir. Girdi özniteliklerini tamamen göz ardı eder.
* **Ne Yapar?**: Tahminlerini yalnızca eğitim setindeki hedef değişkenin genel dağılımına göre yapar:
    * **Sınıflandırma**: Her zaman en sık görülen sınıfı (çoğunluk sınıfı - majority class) tahmin eder.
    * **Regresyon**: Her zaman hedef değişkenin ortalamasını veya medyanını tahmin eder.
* **Amacı**: Geliştirilen daha karmaşık modellerin gerçekten anlamlı bir öğrenme gerçekleştirip gerçekleştirmediğini ölçmek için bir alt sınır performansı belirlemektir. Bir model `Prior`'dan daha iyi performans gösteremiyorsa, veriden değerli bir bilgi çıkaramamış demektir.

---