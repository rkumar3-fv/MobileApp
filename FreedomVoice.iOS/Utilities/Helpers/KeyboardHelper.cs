//using CoreGraphics;
//using UIKit;
//
//namespace FreedomVoice.iOS.Utilities.Helpers
//{
//    public class KeyboardHelper
//    {
//        public enum Status
//        {
//	        Hidden,
//	        Showing,
//	        Shown
//        }
//
//	    private bool IsTracking = false;
//	    private bool IsPerformingForcedLayout = false;
//	    private UIView AdjustingContainer;
//		    	
//	    UIView TrackingView;
//	
//	    private(set) var keyboardStatus: KeyboardStatus = .hidden
//	    private weak var view: UIView?
//	    private var notificationCenter: NotificationCenter
//	    private var layoutBlock: LayoutBlock?
//	    private var layoutBlockWithStatus: LayoutBlockWithStatus?
//
//	    public KeyboardHelper()
//	    {
//		    let trackingView = new KeyboardTrackingView()
//		    trackingView.positionChangedCallback = { [weak self] in
//			    guard let sSelf = self else { return }
//			    if !sSelf.isPerformingForcedLayout {
//				    sSelf.layoutInputAtTrackingViewIfNeeded(forced: false)
//			    }
//		    }
//		    return trackingView
//	    }
//	    
//    }
//
//	internal class KeyboardTrackingView: UIView
//	{
//		UIView ObservedView;
//		CGSize PreferredSize = CGSize.Empty; 
//		
//	
//			var preferredSize: CGSize = .zero {
//			didSet {
//				if oldValue != self.preferredSize {
//					self.invalidateIntrinsicContentSize()
//					self.window?.setNeedsLayout()
//				}
//			}
//		}	
//	}
//
//
//}
//
//
//	/// Определение сигнатуры блока для лейаута
//	public typealias LayoutBlock = (_ bottomMargin: CGFloat) -> Void
//	
//	public typealias LayoutBlockWithStatus = (_ bottomMargin: CGFloat, _ status: KeyboardStatus) -> Void
//	
//	/// Перечисление визуальных состояний клаввиатуры
//	
//	
//	//MARK: - Variables
//	
//	var isTracking = false
//	weak var adjustingContainer: UIView?
//	var isPerformingForcedLayout: Bool = false
//	
//	var trackingView: UIView {
//		return self.keyboardTrackerView
//	}
//	
//	private(set) var keyboardStatus: KeyboardStatus = .hidden
//	private weak var view: UIView?
//	private var notificationCenter: NotificationCenter
//	private var layoutBlock: LayoutBlock?
//	private var layoutBlockWithStatus: LayoutBlockWithStatus?
//	
//	private lazy var keyboardTrackerView: KeyboardTrackingView = {
//		let trackingView = KeyboardTrackingView()
//		trackingView.positionChangedCallback = { [weak self] in
//			guard let sSelf = self else { return }
//			if !sSelf.isPerformingForcedLayout {
//				sSelf.layoutInputAtTrackingViewIfNeeded(forced: false)
//			}
//		}
//		return trackingView
//	}()
//	
//	//MARK: - Inits
//	
//	public init(view: UIView, adjustingContainer: UIView, layoutBlock: LayoutBlock? = nil, layoutBlockWithStatus: LayoutBlockWithStatus? = nil, notificationCenter: NotificationCenter) {
//		self.view = view
//		self.layoutBlock = layoutBlock
//		self.layoutBlockWithStatus = layoutBlockWithStatus
//		self.adjustingContainer = adjustingContainer
//		self.notificationCenter = notificationCenter
//		self.notificationCenter.addObserver(self, selector: #selector(keyboardWillShow(_:)), name: UIResponder.keyboardWillShowNotification, object: nil)
//		self.notificationCenter.addObserver(self, selector: #selector(keyboardDidShow(_:)), name: UIResponder.keyboardDidShowNotification, object: nil)
//		self.notificationCenter.addObserver(self, selector: #selector(keyboardWillHide(_:)), name: UIResponder.keyboardWillHideNotification, object: nil)
//		self.notificationCenter.addObserver(self, selector: #selector(keyboardWillChangeFrame(_:)), name: UIResponder.keyboardWillChangeFrameNotification, object: nil)
//	}
//	
//	deinit {
//		self.notificationCenter.removeObserver(self)
//	}
//	
//	//MARK: - Public
//	
//	public func startTracking() {
//		self.isTracking = true
//	}
//	
//	public func stopTracking() {
//		self.isTracking = false
//	}
//	
//	func adjustTrackingViewSizeIfNeeded() {
//		guard self.isTracking && self.keyboardStatus == .shown else { return }
//		self.adjustTrackingViewSize()
//	}
//	
//	//MARK: - Private
//	
//	@objc private func keyboardWillShow(_ notification: Notification) {
//		guard self.isTracking else { return }
//		guard !self.isPerformingForcedLayout else { return }
//		let bottomConstraint = self.bottomConstraintFromNotification(notification)
//		guard bottomConstraint > 0 else { return }
//		self.keyboardStatus = .showing
//		self.layoutInputContainer(withBottomConstraint: bottomConstraint)
//	}
//	
//	@objc private func keyboardDidShow(_ notification: Notification) {
//		guard self.isTracking else { return }
//		guard !self.isPerformingForcedLayout else { return }
//		
//		let bottomConstraint = self.bottomConstraintFromNotification(notification)
//		guard bottomConstraint > 0 else { return }
//		self.keyboardStatus = .shown
//		self.layoutInputContainer(withBottomConstraint: bottomConstraint)
//		self.adjustTrackingViewSizeIfNeeded()
//	}
//	
//	@objc private func keyboardWillChangeFrame(_ notification: Notification) {
//		guard self.isTracking else { return }
//		let bottomConstraint = self.bottomConstraintFromNotification(notification)
//		if bottomConstraint == 0 {
//			self.keyboardStatus = .hidden
//			self.layoutInputAtBottom()
//		}
//	}
//	
//	@objc private func keyboardWillHide(_ notification: Notification) {
//		guard self.isTracking else { return }
//		self.keyboardStatus = .hidden
//		self.layoutInputAtBottom()
//	}
//	
//	private func bottomConstraintFromNotification(_ notification: Notification) -> CGFloat {
//		guard let rect = ((notification as NSNotification).userInfo?[UIResponder.keyboardFrameEndUserInfoKey] as? NSValue)?.cgRectValue else { return 0 }
//		guard rect.height > 0 else { return 0 }
//		guard let view = self.view else {
//			return 0
//		}
//		
//		let rectInView = view.convert(rect, from: nil)
//		let scale = UIScreen.main.scale
//		guard round(rectInView.maxY * scale) >= round(view.bounds.height * scale) else { return 0 }
//		return max(0, view.bounds.height - rectInView.minY)
//	}
//	
//	private func bottomConstraintFromTrackingView() -> CGFloat {
//		guard self.keyboardTrackerView.superview != nil else { return 0 }
//		guard let view = self.view else { return 0 }
//		
//		let trackingViewRect = view.convert(self.keyboardTrackerView.bounds, from: self.keyboardTrackerView)
//		return max(0, view.bounds.height - trackingViewRect.maxY)
//	}
//	
//	private func adjustTrackingViewSize() {
//		guard let adjustingContainer = self.adjustingContainer else {
//			return
//		}
//		
//		let inputContainerHeight = adjustingContainer.bounds.height
//		if self.keyboardTrackerView.preferredSize.height != inputContainerHeight {
//			self.keyboardTrackerView.preferredSize.height = inputContainerHeight
//			self.isPerformingForcedLayout = true
//			self.keyboardTrackerView.window?.layoutIfNeeded()
//			self.isPerformingForcedLayout = false
//		}
//	}
//	
//	private func layoutInputAtBottom() {
//		self.keyboardTrackerView.bounds.size.height = 0
//		self.layoutInputContainer(withBottomConstraint: 0)
//	}
//	
//	func layoutInputAtTrackingViewIfNeeded(forced: Bool) {
//		guard self.isTracking && (self.keyboardStatus == .shown || forced) else { return }
//		self.layoutInputContainer(withBottomConstraint: self.bottomConstraintFromTrackingView())
//	}
//	
//	private func layoutInputContainer(withBottomConstraint constraint: CGFloat) {
//		self.isPerformingForcedLayout = true
//		self.layoutBlock?(constraint)
//		self.layoutBlockWithStatus?(constraint, keyboardStatus)
//		self.isPerformingForcedLayout = false
//	}
//}
//
//private class KeyboardTrackingView: UIView {
//	
//	//MARK: - Variables
//	
//	var positionChangedCallback: (() -> Void)?
//	var observedView: UIView?
//	
//	var preferredSize: CGSize = .zero {
//		didSet {
//			if oldValue != self.preferredSize {
//				self.invalidateIntrinsicContentSize()
//				self.window?.setNeedsLayout()
//			}
//		}
//	}
//	
//	override var intrinsicContentSize: CGSize {
//		return self.preferredSize
//	}
//	
//	//MARK: - Inits
//	
//	override init(frame: CGRect) {
//		super.init(frame: frame)
//		self.commonInit()
//	}
//	
//	required init?(coder aDecoder: NSCoder) {
//		super.init(coder: aDecoder)
//		self.commonInit()
//	}
//	
//	deinit {
//		if let observedView = self.observedView {
//			observedView.removeObserver(self, forKeyPath: "frame")
//		}
//	}
//	
//	//MARK: - Private
//	
//	private func commonInit() {
//		self.autoresizingMask = .flexibleHeight
//		self.isUserInteractionEnabled = false
//		self.backgroundColor = UIColor.clear
//		self.isHidden = true
//	}
//	
//	override func willMove(toSuperview newSuperview: UIView?) {
//		if let observedView = self.observedView {
//			observedView.removeObserver(self, forKeyPath: "center")
//			self.observedView = nil
//		}
//		
//		if let newSuperview = newSuperview {
//			newSuperview.addObserver(self, forKeyPath: "center", options: [.new, .old], context: nil)
//			self.observedView = newSuperview
//		}
//		
//		super.willMove(toSuperview: newSuperview)
//	}
//	
//	//swiftlint:disable block_based_kvo
//	override func observeValue(forKeyPath keyPath: String?, of object: Any?, change: [NSKeyValueChangeKey: Any]?, context: UnsafeMutableRawPointer?) {
//		guard let object = object as? UIView, let superview = self.superview else { return }
//		if object === superview {
//			guard let sChange = change else { return }
//			let oldCenter = (sChange[NSKeyValueChangeKey.oldKey] as! NSValue).cgPointValue
//			let newCenter = (sChange[NSKeyValueChangeKey.newKey] as! NSValue).cgPointValue
//			if oldCenter != newCenter {
//				self.positionChangedCallback?()
//			}
//		}
//	}
//}
